using CSBugTracker.Models;

namespace CSBugTracker.Services.Interfaces
{
    public interface IBTTicketHistoryService
    {
        Task AddHistoryAsync(Ticket? oldTicket, Ticket? newTicket, string? userId);
        Task AddHistoryAsync(int? ticketId, string? model, string? userId);
        Task<IEnumerable<TicketHistory>> GetProjectTicketHistoryAsync(int? projectId, int? companyId);
        public Task<IEnumerable<TicketHistory>> GetRecentTicketHistoryAsync(int? ticketId);
        public Task<IEnumerable<TicketHistory>> GetCompanyTicketHistoryAsync(int? companyId);
    }
}
