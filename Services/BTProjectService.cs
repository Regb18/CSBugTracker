﻿using Azure;
using CSBugTracker.Data;
using CSBugTracker.Models;
using CSBugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;

namespace CSBugTracker.Services
{
    public class BTProjectService : IBTProjectService
    {
        private readonly ApplicationDbContext _context;

        public BTProjectService(ApplicationDbContext context)
        {
            _context = context;
        }

        #region CRUD Methods
        public async Task<Project> GetProjectAsync(int? projectId, int companyId)
        {
            try
            {
                // Positive verification to ensure this is secure
                Project? project = await _context.Projects.Where(p => p.CompanyId == companyId)
                        .Include(p => p.Company)
                        .Include(p => p.ProjectPriority)
                        .Include(p => p.Tickets).ThenInclude(t => t.DeveloperUser)
                        .Include(p => p.Tickets).ThenInclude(t => t.SubmitterUser)
                        .Include(p => p.Tickets).ThenInclude(t => t.TicketPriority)
                        .Include(p => p.Tickets).ThenInclude(t => t.TicketStatus)
                        .Include(p => p.Tickets).ThenInclude(t => t.TicketType)
                        .Include(p => p.Members)
                        .FirstOrDefaultAsync(m => m.Id == projectId);

                return project!;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task AddProjectAsync(Project project)
        {

            try
            {
                await _context.AddAsync(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task UpdateProjectAsync(Project project)
        {
            try
            {
                _context.Update(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task DeleteProjectAsync(Project project)
        {
            try
            {
                project.Archived = true;

                foreach (Ticket ticket in project.Tickets)
                {
                    ticket.ArchivedByProject = true;
                }

                await UpdateTicketAsync(project);
            }
            catch (Exception)
            {

                throw;
            }

        }


        #endregion



        #region Get Projects Methods
        public async Task<BTUser> GetMyProjectsAsync(string userId)
        {
            try
            {
                BTUser? btuser = await _context.Users.Include(u => u.Projects).ThenInclude(p => p.Tickets)
                                                     .Include(u => u.Projects).ThenInclude(p => p.ProjectPriority)
                                                     .Include(u => u.Projects).ThenInclude(p => p.Members)
                                                     .ThenInclude(u => u.Projects).Include(p => p.Company)
                                                     .FirstOrDefaultAsync(u => u.Id == userId);

                return btuser!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<Project>> GetProjectsAsync(int companyId)
        {
            try
            {
                IEnumerable<Project> projects = await _context.Projects
                                                .Where(p => p.Archived == false && p.CompanyId == companyId)
                                                .Include(p => p.Company)
                                                .Include(p => p.Members)
                                                .Include(p => p.Tickets)
                                                .Include(p => p.ProjectPriority)
                                                .ToListAsync();

                return projects;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<Project>> GetProjectsAsync()
        {
            try
            {
                IEnumerable<Project> projects = await _context.Projects
                                                              .ToListAsync();

                return projects;
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion



        #region Add Members to Project Methods
        public async Task AddProjectToMembersAsync(IEnumerable<string> memberIds, int projectId)
        {
            try
            {
                Project? project = await _context.Projects
                                                 .Include(c => c.Members) // Eager Load
                                                 .FirstOrDefaultAsync(c => c.Id == projectId);

                foreach (string memberId in memberIds)
                {
                    BTUser? member = await _context.Users.FindAsync(memberId);

                    if (project != null && member != null)
                    {
                        // Can use add because we're working with objects
                        project.Members.Add(member);
                    }
                }

                await _context.SaveChangesAsync();

            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<bool> IsTagOnProjectAsync(string memberId, int projectId)
        {
            try
            {
                Project? project = await _context.Projects
                                                 .Include(c => c.Members) // Eager Load
                                                 .FirstOrDefaultAsync(c => c.Id == projectId);

                bool isInMember = project!.Members.Select(c => c.Id).Contains(memberId);

                return isInMember;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RemoveAllProjectMembersAsync(int projectId)
        {
            try
            {
                // c represents an individual contact record in the database
                Project? project = await _context.Projects
                                         .Include(b => b.Members)
                                         .FirstOrDefaultAsync(b => b.Id == projectId);
                if (project != null)
                {
                    // we can do this because we used an ICollection
                    project!.Members.Clear();

                    _context.Update(project);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion



        #region Projects Navigation Properties
        public async Task<IEnumerable<BTUser>> GetMembersAsync(int companyId)
        {
            try
            {
                IEnumerable<BTUser> members = await _context.Users
                                                .Where(u => u.CompanyId == companyId)
                                                .Include(p => p.Company)
                                                .Include(p => p.Projects)
                                                .ToListAsync();

                return members;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<IEnumerable<ProjectPriority>> GetProjectPriosAsync()
        {
            try
            {
                IEnumerable<ProjectPriority> projectPriorities = await _context.ProjectPriorities
                                                                               .ToListAsync();

                return projectPriorities;
            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion
     



    }
}

