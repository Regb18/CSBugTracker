﻿using CSBugTracker.Models;

namespace CSBugTracker.Services.Interfaces
{
	public interface IBTTicketService
	{
        #region CRUD Services
        public Task AddTicketAsync(Ticket ticket);

        public Task UpdateTicketAsync(Ticket ticket);

        public Task DeleteTicketAsync(Ticket ticket);

        public Task<Ticket> GetTicketAsync(int? ticketId, int? companyId);

        #endregion


        // Get Ticketsss (recent tickets, priority, status, type, history length)
        #region Get Tickets Methods
        public Task<IEnumerable<Ticket>> GetTicketsAsync();
        public Task<IEnumerable<Ticket>> GetTicketsAsync(string? userId, int? companyId);
        public Task<IEnumerable<Ticket>> GetTicketsbyProjectsAsync(int? companyId, int? projectId);
        public Task<IEnumerable<Ticket>> GetTicketsbyUserAsync(int? companyId, string? userId);
        public Task<IEnumerable<Ticket>> GetUnassignedTicketsAsync(int? companyId, string? userId);
        public Task<IEnumerable<Ticket>> GetPMUnassignedTicketsAsync(int? companyId, string? userId);
        public Task<IEnumerable<Ticket>> GetArchivedTicketsAsync(string? userId, int? companyId);
        public Task<Ticket> GetTicketAsNoTrackingAsync(int? ticketId, int? companyId);
        #endregion

        #region Get Tickets Developing and Tickets Submitted
        public Task<int> GetTicketsSubmittedCompanyAsync(string? userId, int? companyId);
        public Task<int> GetTicketsDevelopingCompanyAsync(string? userId, int? companyId);
        public Task<int> GetTicketsSubmittedProjectAsync(string? userId, int? projectId);
        public Task<int> GetTicketsDevelopingProjectAsync(string? userId, int? projectId);

        #endregion




        // Add Ticket Comment
        public Task AddCommentAsync(TicketComment comment);


        // Add Ticket Attachment
        public Task AddTicketAttachmentAsync(TicketAttachment ticketAttachment);

        public Task<TicketAttachment> GetTicketAttachmentByIdAsync(int ticketAttachmentId);


        // Get Recent Comments and Attachments
        public Task<IEnumerable<TicketComment>> GetRecentTicketCommentsAsync(int? ticketId);
        public Task<IEnumerable<TicketAttachment>> GetRecentTicketAttachmentsAsync(int? ticketId);


        // Get Ticket Navigation Properties
        public Task<IEnumerable<TicketPriority>> GetTicketPriosAsync();
        public Task<IEnumerable<TicketStatus>> GetTicketStatusesAsync();
        public Task<IEnumerable<TicketType>> GetTicketTypesAsync();


        // Ticket Developer
        public Task<BTUser> GetTicketDeveloperAsync(int? ticketId);
        public Task<bool> AddTicketDeveloperAsync(string? userId, int? ticketId, int? companyId);
        //public Task RemoveTicketDeveloperAsync(int? ticketId);


        public Task<bool> DoesUserHaveTickets(string? userId);

    }
}
