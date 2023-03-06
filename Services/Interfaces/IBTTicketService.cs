using CSBugTracker.Models;

namespace CSBugTracker.Services.Interfaces
{
	public interface IBTTicketService
	{
		// Crud Services

		// Add Ticket

		// Update Ticket

		// Get Ticket
		public Task<Ticket> GetTicketAsync(int? ticketId);

		// Get Ticketsss (recent tickets, users tickets, priority, status, type, history length)

		// Delete(Archive) Project


		// Additional Services

		// Search Tickets

		// Add Ticket Comment
		public Task AddCommentAsync(TicketComment comment, int ticketId);

        // Add Ticket Attachment
        public Task AddTicketAttachmentAsync(TicketAttachment ticketAttachment);

        public Task<TicketAttachment> GetTicketAttachmentByIdAsync(int ticketAttachmentId);
    }
}
