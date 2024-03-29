﻿using CSBugTracker.Models;

namespace CSBugTracker.Services.Interfaces
{
    public interface IBTCompanyService
    {
        public Task<Company> GetCompanyInfoAsync(int? companyId);

        public Task<List<BTUser>> GetMembersAsync(int? companyId);
        public Task<BTUser> GetCompanyAdminAsync(int? companyId);
        public Task<int> GetCompanyTicketCountAsync(int? companyId);
        public Task<int> GetResolvedTicketCountAsync(int? companyId);
        public Task<int> GetArchivedTicketCountAsync(int? companyId);
        public Task<int> GetCompanyProjectCountAsync(int? companyId);



        public Task<int> GetUserProjectsCount(string? userId);
        public Task<int> GetUserTicketsCount(string? userId);
        public Task<int> GetUserNotificationsCount(string? userId);
        public Task<IEnumerable<Notification>> GetUserNotifications(string? userId);
    }
}
