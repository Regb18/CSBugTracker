using System;
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

namespace CSBugTracker.Controllers
{
	public class ProjectsController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly IBTFileService _fileService;
		private readonly UserManager<BTUser> _userManager;
		private readonly IBTProjectService _projectService;

		public ProjectsController(ApplicationDbContext context, 
							      IBTFileService fileService, 
							      UserManager<BTUser> userManager,
                                  IBTProjectService projectService)
		{
			_context = context;
			_fileService = fileService;
			_userManager = userManager;
			_projectService = projectService;
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
			BTUser? btuser = await _projectService.GetMyProjectsAsync(userId);

            // projects based on user/only ones they're in / can only interact with ones they're part of


            return View(btuser);
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

			// make it so you can add a ticket on details with these viewbags
			ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "FullName");
			ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description");
			ViewData["SubmitterUserId"] = new SelectList(_context.Users, "Id", "FullName");
			ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name");
			ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name");
			ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name");
			return View(project);
		}

		// GET: Projects/Create
		public async Task<IActionResult> Create()
		{
			string? userId = _userManager.GetUserId(User);
			BTUser? btuser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
			Company? company = await _context.Companies.Include(c => c.Members).FirstOrDefaultAsync(u => u.Id == btuser!.CompanyId);



			List<BTUser> members = new List<BTUser>();
			
			foreach (BTUser user in company!.Members)
			{
				if (await _userManager.IsInRoleAsync(user, "Developer") || await _userManager.IsInRoleAsync(user, "Submitter"))
				{
					members.Add(user);
				}
			}

			// Can Make Separate Developer and Submitter Lists also / make an onclick for adding
			// this is good for now, need to add in functionality to add multiple members


			ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name");
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
			// add company from user
			string? userId = _userManager.GetUserId(User);
			BTUser? btUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

			if (ModelState.IsValid)
			{


				// Add companyId automatically to Project
				project.CompanyId = btUser!.CompanyId;

				project.Created = DataUtility.GetPostGresDate(DateTime.UtcNow);

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



				_context.Add(project);
				await _context.SaveChangesAsync();


				// Service Call
				await _projectService.AddProjectToMembersAsync(selected, project.Id);


				return RedirectToAction(nameof(Index));
			}
			ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", project.CompanyId);
			ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name", project.ProjectPriorityId);
			return View(project);
		}

		// GET: Projects/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null || _context.Projects == null)
			{
				return NotFound();
			}


			var project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == id);


			if (project == null)
			{
				return NotFound();
			}


			string? userId = _userManager.GetUserId(User);
			BTUser? btuser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
			Company? company = await _context.Companies.Include(c => c.Members).FirstOrDefaultAsync(u => u.Id == btuser!.CompanyId);

			List<BTUser> members = new List<BTUser>();

			foreach (BTUser user in company!.Members)
			{
				if (await _userManager.IsInRoleAsync(user, "Developer") || await _userManager.IsInRoleAsync(user, "Submitter"))
				{
					members.Add(user);
				}
			}

			IEnumerable<string> currentMembers = project!.Members.Select(c => c.Id);

			ViewData["Members"] = new MultiSelectList(members, "Id", "FullName", currentMembers);
			ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name", project.ProjectPriorityId);
			return View(project);
		}

		// POST: Projects/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Created,StartDate,EndDate,ImageFile,ImageData,ImageType,Archived,CompanyId,ProjectPriorityId")] Project project, IEnumerable<string> selected)
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


					_context.Update(project);
					await _context.SaveChangesAsync();


					// Service Call
					await _projectService.RemoveAllProjectMembersAsync(project.Id);

					await _projectService.AddProjectToMembersAsync(selected, project.Id);



				}
				catch (DbUpdateConcurrencyException)
				{
					if (!ProjectExists(project.Id))
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
			//ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", project.CompanyId);
			ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name", project.ProjectPriorityId);
			return View(project);
		}

		// GET: Projects/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null || _context.Projects == null)
			{
				return NotFound();
			}

			Project? project = await _context.Projects
										.Include(p => p.Company)
										.Include(p => p.ProjectPriority)
										.Include(p => p.Tickets)
										.FirstOrDefaultAsync(m => m.Id == id);
			if (project == null)
			{
				return NotFound();
			}

			return View(project);
		}

		// POST: Projects/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			if (_context.Projects == null)
			{
				return Problem("Entity set 'ApplicationDbContext.Projects'  is null.");
			}

			var project = await _context.Projects.Include(p => p.Tickets).FirstOrDefaultAsync(p => p.Id == id);

			try
			{
				project!.Archived = true;

				foreach (Ticket ticket in project.Tickets)
				{
					ticket.Archived = true;
					ticket.ArchivedByProject = true;
				}

				_context.Update(project);
				await _context.SaveChangesAsync();

				return RedirectToAction(nameof(Index));
			}
			catch (Exception)
			{

				throw;
			}

			//if (project != null)
			//{
			//    _context.Projects.Remove(project);
			//}

			//await _context.SaveChangesAsync();
			//return RedirectToAction(nameof(Index));
		}

		private bool ProjectExists(int id)
		{
			return (_context.Projects?.Any(e => e.Id == id)).GetValueOrDefault();
		}
	}
}
