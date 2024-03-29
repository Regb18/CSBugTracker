﻿using CSBugTracker.Data;
using CSBugTracker.Models;
using CSBugTracker.Models.Enums;
using CSBugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CSBugTracker.Services
{
    public class BTCompanyService : IBTCompanyService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTRolesService _rolesService;

        public BTCompanyService(ApplicationDbContext context, IBTRolesService rolesService)
        {
            _context = context;
            _rolesService = rolesService;
        }

        public async Task<Company> GetCompanyInfoAsync(int? companyId)
        {
            try
            {
                Company? company = await _context.Companies.Include(c => c.Members)
                                                           .Include(c => c.Projects).ThenInclude(p => p.Tickets)
                                                           .Include(c => c.Projects).ThenInclude(p => p.ProjectPriority)
                                                           .Include(c => c.Invites)
                                                           .FirstOrDefaultAsync(c => c.Id == companyId);
                return company!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<BTUser>> GetMembersAsync(int? companyId)
        {
            try
            {
                List<BTUser>? members = await _context.Users
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
        public async Task<BTUser> GetCompanyAdminAsync(int? companyId)
        {
            try
            {
                Company? company = await _context.Companies.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == companyId);

                foreach (BTUser member in company!.Members)
                {
                    if (await _rolesService.IsUserInRoleAsync(member, nameof(BTRoles.Admin)))
                    {
                        return member;
                    }
                }

                return null!;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<int> GetCompanyTicketCountAsync(int? companyId)
        {
            try
            {
                IEnumerable<Ticket> tickets = await _context.Tickets
                                                             .Where(t =>  t.Project!.CompanyId == companyId && t.Archived == false && t.ArchivedByProject == false)
                                                             .ToListAsync();
                
                return tickets.Count();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<int> GetResolvedTicketCountAsync(int? companyId)
        {
            try
            {
                IEnumerable<Ticket> tickets = await _context.Tickets
                                                             .Where(t => t.Project!.CompanyId == companyId && t.TicketStatus!.Name == nameof(BTTicketStatuses.Resolved))
                                                             .ToListAsync();

                return tickets.Count();
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<int> GetArchivedTicketCountAsync(int? companyId)
        {
            try
            {
                IEnumerable<Ticket> tickets = await _context.Tickets
                                                             .Where(t => t.Project!.CompanyId == companyId && (t.Archived == true || t.ArchivedByProject == true))
                                                             .ToListAsync();

                return tickets.Count();
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<int> GetCompanyProjectCountAsync(int? companyId)
        {
            try
            {
                IEnumerable<Project> projects = await _context.Projects
                                                             .Where(p => p.CompanyId == companyId && p.Archived == false)
                                                             .ToListAsync();

                return projects.Count();
            }
            catch (Exception)
            {

                throw;
            }
        }


        // GetUserProjectsCount
        public async Task<int> GetUserProjectsCount(string? userId)
        {
            BTUser? user = await _context.Users.FirstOrDefaultAsync(u=>u.Id == userId);

            IEnumerable<Project> projects = await _context.Projects.Where(p => p.Members.Contains(user!)).ToListAsync();

            int count = projects.Count();

            return count;
        }


        // GetUserTicketsCount
        public async Task<int> GetUserTicketsCount(string? userId)
        {
            IEnumerable<Ticket> tickets = await _context.Tickets
                                                        .Where(p => p.SubmitterUserId == userId || p.DeveloperUserId == userId)
                                                        .ToListAsync();

            int count = tickets.Count();

            return count;
        }

        // GetUserNotificationsCount
        public async Task<int> GetUserNotificationsCount(string? userId)
        {
            IEnumerable<Notification> notifications = await _context.Notifications
                                                        .Where(p => p.RecipientId == userId)
                                                        .ToListAsync();

            int count = notifications.Count();

            return count;
        }

        //GetUserNotifications - Include User
        public async Task<IEnumerable<Notification>> GetUserNotifications(string? userId)
        {
            IEnumerable<Notification> notifications = await _context.Notifications
                                                                    .Include(n=>n.Sender)
                                                                    .Include(n => n.Ticket)
                                                                    .Where(p => p.RecipientId == userId)
                                                                    .ToListAsync();

            return notifications;
        }




    }
}
