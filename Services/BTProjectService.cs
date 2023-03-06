using Azure;
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

        ///////////////////////// Add Project to Members

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




    }
}

