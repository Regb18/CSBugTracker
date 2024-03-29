﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CSBugTracker.Data;
using CSBugTracker.Models;
using CSBugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.Net.Sockets;
using NuGet.Protocol;
using CSBugTracker.Extensions;
using Microsoft.AspNetCore.Authorization;
using CSBugTracker.Models.ViewModels;
using CSBugTracker.Models.Enums;

namespace CSBugTracker.Controllers
{
    [Authorize(Roles = "Admin, ProjectManager")]
    public class ProjectsController : Controller
    {
        private readonly IBTFileService _fileService;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTProjectService _projectService;
        private readonly IBTTicketService _ticketService;
        private readonly IBTRolesService _rolesService;
        private readonly IBTCompanyService _companyService;

        public ProjectsController(IBTFileService fileService,
                                  UserManager<BTUser> userManager,
                                  IBTProjectService projectService,
                                  IBTTicketService ticketService,
                                  IBTRolesService rolesService,
                                  IBTCompanyService companyService)
        {
            _fileService = fileService;
            _userManager = userManager;
            _projectService = projectService;
            _ticketService = ticketService;
            _rolesService = rolesService;
            _companyService = companyService;
        }

        // GET: Assign Project Manager
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignPM(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();


            IEnumerable<BTUser> projectManagers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId);
            BTUser? currentPM = await _projectService.GetProjectManagerAsync(id);

            AssignPMViewModel viewModel = new()
            {
                Project = await _projectService.GetProjectAsync(id, companyId),
                PMList = new SelectList(projectManagers, "Id", "FullName", currentPM?.Id),
                PMId = currentPM?.Id
            };


            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // makes sure any requests that are accepted come from within this application
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignPm(AssignPMViewModel viewModel)
        {
            if (!string.IsNullOrEmpty(viewModel.PMId))
            {
                await _projectService.AddProjectManagerAsync(viewModel.PMId, viewModel.Project?.Id);

                // route values need to match up in name - "id" matches asp-route-id
                return RedirectToAction(nameof(Details), new { id = viewModel.Project?.Id });
            }

            ModelState.AddModelError("PMId", "No Project Manager chosen. Please select a PM.");

            int companyId = User.Identity!.GetCompanyId();

            IEnumerable<BTUser> projectManagers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId);
            BTUser? currentPM = await _projectService.GetProjectManagerAsync(viewModel.Project?.Id);
            viewModel.Project = await _projectService.GetProjectAsync(viewModel.Project?.Id, companyId);
            viewModel.PMList = new SelectList(projectManagers, "Id", "FullName", currentPM?.Id);
            viewModel.PMId = currentPM?.Id;

            return View(viewModel);
        }

        // GET: Add Members to Project
        [HttpGet]
        public async Task<IActionResult> AssignProjectMembers(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();

            Project? project = await _projectService.GetProjectAsync(id, companyId);

            List<BTUser> submitters = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Submitter), companyId);
            List<BTUser> developers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId);

            // this method "Concat" returns an IEnumerable and we're declaring a list, so we need to add ToList at the end
            List<BTUser> userList = submitters.Concat(developers).ToList();

            // select lets me choose which property of BTUser I want
            List<string> currentMembers = project.Members.Select(m => m.Id).ToList();



            ProjectMembersViewModel viewModel = new()
            {
                Project = project,
                UsersList = new MultiSelectList(userList, "Id", "FullName", currentMembers),
            };


            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignProjectMembers(ProjectMembersViewModel viewModel)
        {
            int companyId = User.Identity!.GetCompanyId();

            if (viewModel.SelectedMembers != null)
            {
                await _projectService.RemoveAllProjectMembersAsync(viewModel.Project!.Id, companyId);

                // await allows the async method to run because it's saying it'll wait for the result while the async is doing it's thing and then we'll keep going
                await _projectService.AddProjectToMembersAsync(viewModel.SelectedMembers!, viewModel.Project!.Id, companyId);


                return RedirectToAction(nameof(Details), new { id = viewModel.Project?.Id });
            
            }

            ModelState.AddModelError("SelectedMembers", "No Users chosen. Please select Users.");
            // Reset the Form
            viewModel.Project = await _projectService.GetProjectAsync(viewModel.Project!.Id, companyId);
            List<string> currentMembers = viewModel.Project.Members.Select(m => m.Id).ToList();

            List<BTUser> submitters = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Submitter), companyId);
            List<BTUser> developers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId);
            List<BTUser> userList = submitters.Concat(developers).ToList();

            viewModel.UsersList = new MultiSelectList(userList, "Id", "FullName", currentMembers);

            return View(viewModel);

        }


        // GET: Projects
        public async Task<IActionResult> Index()
        {
            int companyId = User.Identity!.GetCompanyId();

            IEnumerable<Project> projects = await _projectService.GetProjectsAsync(companyId);

            return View(projects);
        }

        // GET: My Projects
        public async Task<IActionResult> MyProjects()
        {
            string userId = _userManager.GetUserId(User)!;
            int companyId = User.Identity!.GetCompanyId();

            IEnumerable<Project>? projects = await _projectService.GetMyProjectsAsync(userId, companyId);

            // projects based on user/only ones they're in / can only interact with ones they're part of


            return View(projects);
        }


        // Get Archived Projects
        public async Task<IActionResult> ArchivedProjects()
        {
            int companyId = User.Identity!.GetCompanyId();

            IEnumerable<Project> projects = await _projectService.GetArchivedProjectsAsync(companyId);

            return View(projects);
        }


        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // this happens on the server, so it can't be tampered with
            int companyId = User.Identity!.GetCompanyId();

            Project? project = await _projectService.GetProjectAsync(id, companyId);

            if (project == null)
            {
                return NotFound();
            }


            ///////// For ADDING TICKETS ON DETAILS PAGE ///////////////
            // TODO: Add In Later?

            //         List<BTUser> members = new List<BTUser>();

            //         foreach (BTUser user in await _projectService.GetMembersAsync(companyId))
            //         {
            //             if (await _userManager.IsInRoleAsync(user, "Developer"))
            //             {
            //                 members.Add(user);
            //             }
            //         }
            //         ViewData["DeveloperUserId"] = new SelectList(members, "Id", "FullName");
            //ViewData["ProjectId"] = new SelectList(await _projectService.GetProjectsAsync(companyId), "Id", "Name");
            //ViewData["TicketPriorityId"] = new SelectList(await _ticketService.GetTicketPriosAsync(), "Id", "Name");
            //ViewData["TicketStatusId"] = new SelectList(await _ticketService.GetTicketStatusesAsync(), "Id", "Name");
            //ViewData["TicketTypeId"] = new SelectList(await _ticketService.GetTicketTypesAsync(), "Id", "Name");

            return View(project);
        }

        // GET: Projects/Create
        public async Task<IActionResult> Create()
        {
            int companyId = User.Identity!.GetCompanyId();

            List<BTUser> members = new List<BTUser>();

            foreach (BTUser user in await _companyService.GetMembersAsync(companyId))
            {
                if (await _rolesService.IsUserInRoleAsync(user, nameof(BTRoles.Developer)) || await _rolesService.IsUserInRoleAsync(user, nameof(BTRoles.Submitter)))
                {
                    members.Add(user);
                }
            }

            // Can Make Separate Developer and Submitter Lists also / make an onclick for adding
            // this is good for now, need to add in functionality to add multiple members


            ViewData["ProjectPriorityId"] = new SelectList(await _projectService.GetProjectPriosAsync(), "Id", "Name");
            ViewData["Members"] = new MultiSelectList(members, "Id", "FullName");
            return View(new Project());
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Created,StartDate,EndDate,ImageFile,ImageData,ImageType,Archived,CompanyId,ProjectPriorityId")] Project project, IEnumerable<string> selected)
        {

            if (ModelState.IsValid)
            {

                // add company from user
                // Add companyId automatically to Project
                project.CompanyId = User.Identity!.GetCompanyId();

                project.Created = DataUtility.GetPostGresDate(DateTime.Now);

                if (project.StartDate != null)
                {
                    project.StartDate = DataUtility.GetPostGresDate(project.StartDate.Value);
                }

                if (project.EndDate != null)
                {
                    project.EndDate = DataUtility.GetPostGresDate(project.EndDate.Value);
                }


                if (project.ImageFile != null)
                {
                    project.ImageData = await _fileService.ConvertFileToByteArrayAsync(project.ImageFile);
                    project.ImageType = project.ImageFile.ContentType;
                }


                // Service
                await _projectService.AddProjectAsync(project);


                // Service Call
                await _projectService.AddProjectToMembersAsync(selected, project.Id, project.CompanyId);


                return RedirectToAction(nameof(Index));
            }

            return View(project);
        }

        // GET: Projects/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();

            Project? project = await _projectService.GetProjectAsync(id, companyId);

            if (project == null)
            {
                return NotFound();
            }

            ViewData["ProjectPriorityId"] = new SelectList(await _projectService.GetProjectPriosAsync(), "Id", "Name", project.ProjectPriorityId);
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Created,StartDate,EndDate,ImageFile,ImageData,ImageType,Archived,CompanyId,ProjectPriorityId")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {

                    project.Created = DataUtility.GetPostGresDate(project.Created);

                    if (project.StartDate != null)
                    {
                        project.StartDate = DataUtility.GetPostGresDate(project.StartDate.Value);
                    }

                    if (project.EndDate != null)
                    {
                        project.EndDate = DataUtility.GetPostGresDate(project.EndDate.Value);
                    }


                    if (project.ImageFile != null)
                    {
                        project.ImageData = await _fileService.ConvertFileToByteArrayAsync(project.ImageFile);
                        project.ImageType = project.ImageFile.ContentType;
                    }

                    // Add Service
                    await _projectService.UpdateProjectAsync(project);

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await ProjectExists(project.Id))
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

            return View(project);
        }

        // GET: Projects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();

            Project? project = await _projectService.GetProjectAsync(id, companyId);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (id == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Projects'  is null.");
            }

            int companyId = User.Identity!.GetCompanyId();

            Project? project = await _projectService.GetProjectAsync(id, companyId);

            try
            {
                await _projectService.DeleteProjectAsync(project);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {

                throw;
            }

        }

        private async Task<bool> ProjectExists(int id)
        {
            return (await _projectService.GetProjectsAsync()).Any(e => e.Id == id);
        }
    }
}
