using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CSBugTracker.Data;
using CSBugTracker.Models;
using Microsoft.AspNetCore.Identity;
using CSBugTracker.Services.Interfaces;

namespace CSBugTracker.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTTicketService _ticketService;

        public TicketsController(ApplicationDbContext context, UserManager<BTUser> userManager, IBTTicketService ticketService)
        {
            _context = context;
            _userManager = userManager;
            _ticketService = ticketService;
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            string userId = _userManager.GetUserId(User)!;
			BTUser? btuser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

			List<Project> projects = await _context.Projects
													  .Where(p => p.CompanyId == btuser!.CompanyId)
													  .Include(p => p.Company)
													  .Include(p => p.Tickets).ThenInclude(t => t.DeveloperUser)
													  .Include(p => p.Tickets).ThenInclude(t => t.SubmitterUser)
													  .Include(p => p.Tickets).ThenInclude(t => t.TicketPriority)
													  .Include(p => p.Tickets).ThenInclude(t => t.TicketStatus)
													  .Include(p => p.Tickets).ThenInclude(t => t.TicketType)
													  .ToListAsync();
            return View(projects);
        }

		// GET: Tickets
		public async Task<IActionResult> MyTickets()
		{
			string userId = _userManager.GetUserId(User)!;


			List<Ticket> tickets = await _context.Tickets.Where(t => t.SubmitterUserId == userId || t.DeveloperUserId == userId)
														 .Include(t => t.DeveloperUser)
														 .Include(t => t.Project)
														 .Include(t => t.SubmitterUser)
														 .Include(t => t.TicketPriority)
														 .Include(t => t.TicketStatus)
														 .Include(t => t.TicketType)
														 .ToListAsync();
			return View(tickets);
		}

		// GET: Tickets/Details/5
		public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Tickets == null)
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

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddTicketComment([Bind("Id,Comment,Created,TicketId,UserId")] TicketComment ticketComment, int ticketId)
        {
			Ticket? ticket = await _ticketService.GetTicketAsync(ticketId);
			ModelState.Remove("UserId");

			if (ModelState.IsValid)
			{

				ticketComment.UserId = _userManager.GetUserId(User);

				if (User.Identity!.IsAuthenticated == true)
				{
					// Automatically assign author and blogpost
					ticketComment.TicketId = ticket!.Id;

					ticketComment.Created = DateTime.UtcNow;

					await _ticketService.AddCommentAsync(ticketComment, ticketId);

					return RedirectToAction(nameof(Details), ticket);
				}
			}

            return RedirectToAction(nameof(Details), ticket);
		}



        // GET: Tickets/Create
        public async Task<IActionResult> Create()
        {

			string? userId = _userManager.GetUserId(User);
			BTUser? btuser = await _context.Users.Include(u => u.Company)
									                .ThenInclude(c => c!.Members)
									             .FirstOrDefaultAsync(u => u.Id == userId);
			//Company? company = await _context.Companies.Include(c => c.Members).FirstOrDefaultAsync(u => u.Id == btuser!.CompanyId);



			List<BTUser> members = new List<BTUser>();

			foreach (BTUser user in btuser!.Company!.Members)
			{
				if (await _userManager.IsInRoleAsync(user, "Developer"))
				{
					members.Add(user);
				}
			}


			ViewData["DeveloperUserId"] = new SelectList(members, "Id", "FullName");
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description"); 
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name");
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Created,Updated,Archived,ArchivedByProject,ProjectId,TicketTypeId,TicketStatusId,TicketPriorityId,DeveloperUserId,SubmitterUserId")] Ticket ticket, int projectId)
        {
            Project? project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
            //ModelState.Remove("ProjectId");
            ModelState.Remove("SubmitterUserId");
            ModelState.Remove("ArchivedByProject");

            if (ModelState.IsValid)
            {
                ticket.ProjectId = projectId;

                ticket.SubmitterUserId = _userManager.GetUserId(User);

                ticket.Created = DataUtility.GetPostGresDate(DateTime.UtcNow);


                _context.Add(ticket);
                await _context.SaveChangesAsync();


                return RedirectToAction(nameof(Details), "Projects", project);

            }


            ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "FullName", ticket.DeveloperUserId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description", ticket.ProjectId);
            //ViewData["SubmitterUserId"] = new SelectList(_context.Users, "Id", "FullName", ticket.SubmitterUserId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            Ticket? ticket = await _context.Tickets.Include(t => t.DeveloperUser)
                                                   .Include(t => t.Project)
                                                   .Include(t => t.SubmitterUser)
                                                   .Include(t => t.TicketPriority)
                                                   .Include(t => t.TicketStatus)
                                                   .Include(t => t.TicketType)
                                                   .FirstOrDefaultAsync(m => m.Id == id);

			string? userId = _userManager.GetUserId(User);
			BTUser? btuser = await _context.Users.Include(u=>u.Company)
                                                    .ThenInclude(c=>c!.Members)
                                                 .FirstOrDefaultAsync(u => u.Id == userId);
			//Company? company = await _context.Companies.Include(c => c.Members).FirstOrDefaultAsync(u => u.Id == btuser!.CompanyId);

			List<BTUser> members = new List<BTUser>();

			foreach (BTUser user in btuser!.Company!.Members)
			{
				if (await _userManager.IsInRoleAsync(user, "Developer"))
				{
					members.Add(user);
				}
			}

			if (ticket == null)
            {
                return NotFound();
            }


            ViewData["DeveloperUserId"] = new SelectList(members, "Id", "FullName", ticket.DeveloperUserId);
            //ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description", ticket.ProjectId);
            //ViewData["SubmitterUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.SubmitterUserId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);

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
                try
                {

                    ticket.Created = DataUtility.GetPostGresDate(ticket.Created);
                    ticket.Updated = DataUtility.GetPostGresDate(DateTime.UtcNow);


                    _context.Update(ticket);

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.DeveloperUserId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description", ticket.ProjectId);
            ViewData["SubmitterUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.SubmitterUserId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.DeveloperUser)
                .Include(t => t.Project)
                .Include(t => t.SubmitterUser)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Tickets == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Tickets'  is null.");
            }

            var ticket = await _context.Tickets.FindAsync(id);


            try
            {
                ticket!.Archived = true;

                _context.Update(ticket);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {

                throw;
            }



            //if (ticket != null)
            //{
            //    _context.Tickets.Remove(ticket);
            //}

            //await _context.SaveChangesAsync();
            //return RedirectToAction(nameof(Index));
        }

        private bool TicketExists(int id)
        {
          return (_context.Tickets?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
