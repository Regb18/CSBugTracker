using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CSBugTracker.Data;
using CSBugTracker.Models;
using Microsoft.AspNetCore.Authorization;
using CSBugTracker.Models.ViewModels;
using CSBugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using CSBugTracker.Extensions;
using CSBugTracker.Models.Enums;

namespace CSBugTracker.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CompaniesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTFileService _fileService;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTProjectService _projectService;
        private readonly IBTRolesService _rolesService;
        private readonly IBTCompanyService _companyService;

        public CompaniesController(ApplicationDbContext applicationDbContext,
                                  IBTFileService fileService,
                                  UserManager<BTUser> userManager,
                                  IBTProjectService projectService,
                                  IBTRolesService rolesService,
                                  IBTCompanyService companyService)
        {
            _context = applicationDbContext;
            _fileService = fileService;
            _userManager = userManager;
            _projectService = projectService;
            _rolesService = rolesService;
            _companyService = companyService;
        }



        [HttpGet]
        public async Task<IActionResult> ManageUserRoles()
        {
            List<ManageUserRolesViewModel> viewModels = new List<ManageUserRolesViewModel>();

            int companyId = User.Identity!.GetCompanyId();

            List<BTUser> members = await _companyService.GetMembersAsync(companyId);


            foreach (BTUser user in members)
            {
                if (!await _rolesService.IsUserInRoleAsync(user, nameof(BTRoles.Admin)))
                {
                    ManageUserRolesViewModel viewModel = new()
                    {
                        BTUser = user,
                        Roles = new MultiSelectList(await _rolesService.GetRolesAsync(), await _rolesService.GetUserRolesAsync(user)),
                        SelectedRoles = (await _rolesService.GetUserRolesAsync(user)).ToList()
                    };

                    viewModels.Add(viewModel);
                }
            }

            return View(viewModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageUserRoles(ManageUserRolesViewModel viewModel)
        {
            //int companyId = User.Identity!.GetCompanyId();
            BTUser? user = await _context.Users.FindAsync(viewModel.BTUser!.Id);

            // Get roles for the user
            List<string> currentRoles = (await _rolesService.GetUserRolesAsync(user!)).ToList();

            await _rolesService.RemoveUserFromRolesAsync(user!, currentRoles);

            foreach (string selectedRole in viewModel.SelectedRoles!)
            {
                if (!await _rolesService.IsUserInRoleAsync(user!, selectedRole))
                {
                    await _rolesService.AddUserToRoleAsync(user!, selectedRole);

                }

            }

            //ModelState.AddModelError("SelectedRoles", "No Role chosen. Please select a Role.");

            return RedirectToAction("PortoIndex", "Home");
        }


        // GET: Companies
        public async Task<IActionResult> Index()
        {
            return _context.Companies != null ?
                        View(await _context.Companies.ToListAsync()) :
                        Problem("Entity set 'ApplicationDbContext.Companies'  is null.");
        }

        // GET: Companies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Companies == null)
            {
                return NotFound();
            }

            var company = await _context.Companies
                .Include(c => c.Projects)
                .Include(c => c.Members)
                .Include(c => c.Invites)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        // GET: Companies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Companies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,ImageData,ImageType")] Company company)
        {
            if (ModelState.IsValid)
            {
                _context.Add(company);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(company);
        }

        // GET: Companies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Companies == null)
            {
                return NotFound();
            }

            var company = await _context.Companies.FindAsync(id);
            if (company == null)
            {
                return NotFound();
            }
            return View(company);
        }

        // POST: Companies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,ImageData,ImageType")] Company company)
        {
            if (id != company.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(company);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompanyExists(company.Id))
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
            return View(company);
        }

        // GET: Companies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Companies == null)
            {
                return NotFound();
            }

            var company = await _context.Companies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        // POST: Companies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Companies == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Companies'  is null.");
            }
            var company = await _context.Companies.FindAsync(id);
            if (company != null)
            {
                _context.Companies.Remove(company);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CompanyExists(int id)
        {
            return (_context.Companies?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
