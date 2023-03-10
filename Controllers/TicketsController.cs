using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CSBugTracker.Data;
using CSBugTracker.Models;
using Microsoft.AspNetCore.Identity;
using CSBugTracker.Services.Interfaces;
using CSBugTracker.Services;
using System.Net.Sockets;
using CSBugTracker.Extensions;
using System.ComponentModel.Design;
using CSBugTracker.Models.Enums;
using CSBugTracker.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace CSBugTracker.Controllers
{
    public class TicketsController : Controller
    {
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTTicketService _ticketService;
        private readonly IBTFileService _fileService;
        private readonly IBTProjectService _projectService;
        private readonly IBTCompanyService _companyService;
        private readonly IBTRolesService _rolesService;
        private readonly IBTTicketHistoryService _historyService;
        private readonly ApplicationDbContext _context;

        public TicketsController(UserManager<BTUser> userManager,
                                 IBTTicketService ticketService,
                                 IBTFileService fileService,
                                 IBTProjectService projectService,
                                 IBTCompanyService companyService,
                                 IBTRolesService rolesService,
                                 IBTTicketHistoryService historyService,
                                 ApplicationDbContext context)
        {
            _userManager = userManager;
            _ticketService = ticketService;
            _fileService = fileService;
            _projectService = projectService;
            _companyService = companyService;
            _rolesService = rolesService;
            _historyService = historyService;
            _context = context;
        }

        // GET: Assign Developer to Ticket
        [HttpGet]
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> AssignDev(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //int companyId = User.Identity!.GetCompanyId();
            //IEnumerable<BTUser> developers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId);
            Ticket? ticket = await _ticketService.GetTicketAsync(id);

            List<BTUser> developers = await _projectService.GetProjectDevelopersAsync(ticket.ProjectId);

            BTUser? currentDev = await _ticketService.GetTicketDeveloperAsync(id);

            AddDevToTicketViewModel viewModel = new()
            {
                Ticket = await _ticketService.GetTicketAsync(id),
                DevList = new SelectList(developers, "Id", "FullName", currentDev?.Id),
                DevId = currentDev?.Id
            };


            return View(viewModel);
        }

        // POST: Assign Dev
        [HttpPost]
        [ValidateAntiForgeryToken] // makes sure any requests that are accepted come from within this application
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> AssignDev(AddDevToTicketViewModel viewModel)
        {
            if (viewModel.DevId != null)
            {
                // Get AsNoTracking
                int companyId = User.Identity!.GetCompanyId();

                Ticket oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(viewModel.Ticket!.Id, companyId);

                try
                {
                    await _ticketService.AddTicketDeveloperAsync(viewModel.DevId, viewModel.Ticket?.Id);
                }
                catch (Exception)
                {

                    throw;
                }

                // Add Ticket History 
                string? userId = _userManager.GetUserId(User);
                Ticket? newTicket = await _ticketService.GetTicketAsNoTrackingAsync(viewModel.Ticket!.Id, companyId);

                await _historyService.AddHistoryAsync(oldTicket, newTicket, userId);



                return RedirectToAction(nameof(Index));
            }



            return RedirectToAction(nameof(AssignDev), new { id = viewModel.Ticket!.Id});
        }


        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            int companyId = User.Identity!.GetCompanyId();

            IEnumerable<Project> projectsTickets = await _ticketService.GetTicketsbyProjectsAsync(companyId);

            return View(projectsTickets);
        }

        // GET: Tickets
        public async Task<IActionResult> MyTickets()
        {
            string userId = _userManager.GetUserId(User)!;

            IEnumerable<Ticket> tickets = await _ticketService.GetTicketsbyUserAsync(userId);

            return View(tickets);
        }


        // GET: Unassigned Tickets
        [HttpGet]
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> UnassignedTickets()
        {
            int companyId = User.Identity!.GetCompanyId();
            string userId = _userManager.GetUserId(User)!;


            if (User.IsInRole(nameof(BTRoles.Admin)))
            {

                IEnumerable<Project> tickets = await _ticketService.GetTicketsbyProjectsAsync(companyId);

                return View(tickets);

            }
            else if (User.IsInRole(nameof(BTRoles.ProjectManager)))
            {

                IEnumerable<Project> tickets = await _ticketService.GetUnassignedTicketsAsync(companyId, userId);

                return View(tickets);

            }


            return RedirectToAction(nameof(Index));
        }



        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Ticket? ticket = await _ticketService.GetTicketAsync(id);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // Post: Ticket Comment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketComment([Bind("Id,Comment,Created,TicketId,UserId")] TicketComment ticketComment)
        {
            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {

                ticketComment.UserId = _userManager.GetUserId(User);

                if (User.Identity!.IsAuthenticated == true)
                {

                    ticketComment.Created = DataUtility.GetPostGresDate(DateTime.UtcNow);

                    await _ticketService.AddCommentAsync(ticketComment);

                    // Add history
                    await _historyService.AddHistoryAsync(ticketComment.TicketId, nameof(TicketComment), ticketComment.UserId);


                }
            }

            return RedirectToAction("Details", new { id = ticketComment.TicketId });
        }

        // Post: Ticket Attachment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketAttachment([Bind("Id,FormFile,FileData,FileContentType,FileName,Description,UserId,TicketId")] TicketAttachment ticketAttachment)
        {
            ModelState.Remove("UserId");

            string statusMessage;

            if (ModelState.IsValid && ticketAttachment.FormFile != null)
            {
                ticketAttachment.FileData = await _fileService.ConvertFileToByteArrayAsync(ticketAttachment.FormFile);
                ticketAttachment.FileName = ticketAttachment.FormFile.FileName;
                ticketAttachment.FileContentType = ticketAttachment.FormFile.ContentType;

                ticketAttachment.Created = DataUtility.GetPostGresDate(DateTime.UtcNow);
                ticketAttachment.UserId = _userManager.GetUserId(User);

                await _ticketService.AddTicketAttachmentAsync(ticketAttachment);

                // Add History
                await _historyService.AddHistoryAsync(ticketAttachment.TicketId, nameof(TicketAttachment), ticketAttachment.UserId);


                statusMessage = "Success: New attachment added to Ticket.";
            }
            else
            {
                statusMessage = "Error: Invalid data.";

            }

            return RedirectToAction("Details", new { id = ticketAttachment.TicketId, message = statusMessage });
        }


        public async Task<IActionResult> ShowFile(int id)
        {
            TicketAttachment ticketAttachment = await _ticketService.GetTicketAttachmentByIdAsync(id);
            string fileName = ticketAttachment.FileName!;
            byte[] fileData = ticketAttachment.FileData!;
            string ext = Path.GetExtension(fileName).Replace(".", "");

            Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
            return File(fileData, $"application/{ext}");
        }


        // GET: Tickets/Create
        public async Task<IActionResult> Create()
        {
            int companyId = User.Identity!.GetCompanyId();

            List<BTUser> members = new List<BTUser>();

            foreach (BTUser user in await _companyService.GetMembersAsync(companyId))
            {
                if (await _userManager.IsInRoleAsync(user, "Developer"))
                {
                    members.Add(user);
                }
            }


            ViewData["DeveloperUserId"] = new SelectList(members, "Id", "FullName");
            ViewData["ProjectId"] = new SelectList(await _projectService.GetProjectsAsync(companyId), "Id", "Name");
            ViewData["TicketPriorityId"] = new SelectList(await _ticketService.GetTicketPriosAsync(), "Id", "Name");
            ViewData["TicketStatusId"] = new SelectList(await _ticketService.GetTicketStatusesAsync(), "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(await _ticketService.GetTicketTypesAsync(), "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Created,Updated,Archived,ArchivedByProject,ProjectId,TicketTypeId,TicketStatusId,TicketPriorityId,DeveloperUserId,SubmitterUserId")] Ticket ticket)
        {

            //ModelState.Remove("ProjectId");
            ModelState.Remove("SubmitterUserId");
            ModelState.Remove("ArchivedByProject");

            if (ModelState.IsValid)
            {
                string? userId = _userManager.GetUserId(User);

                ticket.SubmitterUserId = userId;

                ticket.Created = DataUtility.GetPostGresDate(DateTime.Now);

                // set status id = "new"


                await _ticketService.AddTicketAsync(ticket);


                // Add History Record
                int companyId = User.Identity!.GetCompanyId();
                Ticket? newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, companyId);

                await _historyService.AddHistoryAsync(null, newTicket, userId);



                return RedirectToAction("Details", new { id = ticket.Id });

            }

            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Ticket? ticket = await _ticketService.GetTicketAsync(id);

            if (ticket == null)
            {
                return NotFound();
            }



            int companyId = User.Identity!.GetCompanyId();

            List<BTUser> members = new List<BTUser>();

            foreach (BTUser user in await _companyService.GetMembersAsync(companyId))
            {
                if (await _userManager.IsInRoleAsync(user, "Developer"))
                {
                    members.Add(user);
                }
            }


            ViewData["DeveloperUserId"] = new SelectList(members, "Id", "FullName", ticket.DeveloperUserId);
            ViewData["TicketPriorityId"] = new SelectList(await _ticketService.GetTicketPriosAsync(), "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(await _ticketService.GetTicketStatusesAsync(), "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(await _ticketService.GetTicketTypesAsync(), "Id", "Name", ticket.TicketTypeId);

            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Created,Updated,Archived,ArchivedByProject,ProjectId,TicketTypeId,TicketStatusId,TicketPriorityId,DeveloperUserId,SubmitterUserId")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {

                int companyId = User.Identity!.GetCompanyId();
                string? userId = _userManager.GetUserId(User);
                Ticket? oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, companyId);


                try
                {

                    ticket.Created = DataUtility.GetPostGresDate(ticket.Created);
                    ticket.Updated = DataUtility.GetPostGresDate(DateTime.Now);


                    await _ticketService.UpdateTicketAsync(ticket);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await TicketExists(ticket.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                // Add History
                Ticket? newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, companyId);

                // Add Ticket History
                await _historyService.AddHistoryAsync(oldTicket, newTicket, userId);


                return RedirectToAction(nameof(Index));
            }


            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Ticket? ticket = await _ticketService.GetTicketAsync(id);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (id == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Tickets'  is null.");
            }

            Ticket? ticket = await _ticketService.GetTicketAsync(id);


            try
            {
                await _ticketService.DeleteTicketAsync(ticket);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {

                throw;
            }

        }

        private async Task<bool> TicketExists(int id)
        {
            return (await _ticketService.GetTicketsAsync()).Any(b => b.Id == id);
        }
    }
}
