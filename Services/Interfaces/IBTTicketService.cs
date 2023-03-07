using CSBugTracker.Models;

namespace CSBugTracker.Services.Interfaces
{
	public interface IBTTicketService
	{
        #region CRUD Services
        public Task AddTicketAsync(Ticket ticket);

        public Task UpdateTicketAsync(Ticket ticket);

        public Task DeleteTicketAsync(Ticket ticket);

        public Task<Ticket> GetTicketAsync(int? ticketId);

        #endregion


        // Get Ticketsss (recent tickets, priority, status, type, history length)
        #region Get Tickets Methods
        public Task<IEnumerable<Project>> GetTicketsbyProjectsAsync(int companyId);
        public Task<IEnumerable<Ticket>> GetTicketsbyUserAsync(string userId);
        public Task<IEnumerable<Ticket>> GetTicketsAsync();
        public Task<IEnumerable<Ticket>> GetTicketsAsync(int companyId);
        #endregion

        // Additional Services

        // Search Tickets

        // Add Ticket Comment
        public Task AddCommentAsync(TicketComment comment);

        // Add Ticket Attachment
        public Task AddTicketAttachmentAsync(TicketAttachment ticketAttachment);

        public Task<TicketAttachment> GetTicketAttachmentByIdAsync(int ticketAttachmentId);

		// Get Ticket Navigation Properties
		public Task<IEnumerable<TicketPriority>> GetTicketPriosAsync();
        public Task<IEnumerable<TicketStatus>> GetTicketStatusesAsync();
        public Task<IEnumerable<TicketType>> GetTicketTypesAsync();
    }
}
