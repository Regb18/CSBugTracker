using CSBugTracker.Data;
using CSBugTracker.Models;
using CSBugTracker.Models.Enums;
using CSBugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CSBugTracker.Services
{
	public class BTTicketService : IBTTicketService
	{
		private readonly ApplicationDbContext _context;

		public BTTicketService(ApplicationDbContext context)
		{
			_context = context;
		}


        #region CRUD Services
        public async Task<Ticket> GetTicketAsync(int? ticketId)
		{
			try
			{
				Ticket? ticket = await _context.Tickets
											   .Include(t => t.DeveloperUser)
											   .Include(t => t.Project)
											   .Include(t => t.SubmitterUser)
											   .Include(t => t.TicketPriority)
											   .Include(t => t.TicketStatus)
											   .Include(t => t.TicketType)
											   .Include(t => t.TicketComments).ThenInclude(c => c.User)
											   .Include(t => t.TicketAttachments).ThenInclude(c => c.User)
                                               .Include(t => t.TicketHistory)
											   .FirstOrDefaultAsync(m => m.Id == ticketId);

				return ticket!;
			}
			catch (Exception)
			{

				throw;
			}
		}

        public async Task AddTicketAsync(Ticket ticket)
        {

            try
            {
                await _context.AddAsync(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task UpdateTicketAsync(Ticket ticket)
        {
            try
            {
                _context.Update(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task DeleteTicketAsync(Ticket ticket)
        {
            try
            {
                ticket.Archived = true;
                await UpdateTicketAsync(ticket);
            }
            catch (Exception)
            {

                throw;
            }

        }
        #endregion



        #region Get Tickets Methods
        public async Task<IEnumerable<Ticket>> GetTicketsAsync()
        {
            try
            {
                IEnumerable<Ticket> tickets = await _context.Tickets
                                                            .ToListAsync();

                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<IEnumerable<Ticket>> GetTicketsAsync(int companyId)
        {
            try
            {
                IEnumerable<Project> projectsTickets = await _context.Projects
                                                          .Where(p => p.CompanyId == companyId)                                                         
                                                          .Include(p => p.Company)
                                                          .Include(p => p.Tickets).ThenInclude(t => t.DeveloperUser)
                                                          .Include(p => p.Tickets).ThenInclude(t => t.SubmitterUser)
                                                          .Include(p => p.Tickets).ThenInclude(t => t.TicketPriority)
                                                          .Include(p => p.Tickets).ThenInclude(t => t.TicketStatus)
                                                          .Include(p => p.Tickets).ThenInclude(t => t.TicketType)
                                                          .ToListAsync();

                List<Ticket> tickets = new List<Ticket>();

                foreach (Project project in projectsTickets)
                {
                    foreach (Ticket ticket in project.Tickets)
                    {
                        tickets.Add(ticket);
                    }

                }
                
                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<Project>> GetTicketsbyProjectsAsync(int companyId)
        {
            try
            {
                IEnumerable<Project> projectsTickets = await _context.Projects
                                                          .Where(p => p.CompanyId == companyId)
                                                          .Include(p => p.Company)
                                                          .Include(p => p.Tickets).ThenInclude(t => t.DeveloperUser)
                                                          .Include(p => p.Tickets).ThenInclude(t => t.SubmitterUser)
                                                          .Include(p => p.Tickets).ThenInclude(t => t.TicketPriority)
                                                          .Include(p => p.Tickets).ThenInclude(t => t.TicketStatus)
                                                          .Include(p => p.Tickets).ThenInclude(t => t.TicketType)
                                                          .ToListAsync();

                return projectsTickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<Ticket>> GetTicketsbyUserAsync(string userId)
        {
            try
            {
                IEnumerable<Ticket> tickets = await _context.Tickets.Where(t => t.SubmitterUserId == userId || t.DeveloperUserId == userId)
                                                             .Include(t => t.DeveloperUser)
                                                             .Include(t => t.Project)
                                                             .Include(t => t.SubmitterUser)
                                                             .Include(t => t.TicketPriority)
                                                             .Include(t => t.TicketStatus)
                                                             .Include(t => t.TicketType)
                                                             .ToListAsync();

                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion



        #region Ticket Attachments / Comments
        public async Task AddTicketAttachmentAsync(TicketAttachment ticketAttachment)
        {
            try
            {
                await _context.AddAsync(ticketAttachment);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<TicketAttachment> GetTicketAttachmentByIdAsync(int ticketAttachmentId)
        {
            try
            {
                TicketAttachment? ticketAttachment = await _context.TicketAttachments
                                                                  .Include(t => t.User)
                                                                  .FirstOrDefaultAsync(t => t.Id == ticketAttachmentId);
                return ticketAttachment!;
            }
            catch (Exception)
            {

                throw;
            }
        }
		public async Task AddCommentAsync(TicketComment comment)
		{

			try
			{
				await _context.AddAsync(comment);
				//ticket!.TicketComments.Add(comment);

				await _context.SaveChangesAsync();
			}
			catch (Exception)
			{

				throw;
			}
		}
        #endregion


       
        #region Ticket Navigation Properties
        public async Task<IEnumerable<TicketPriority>> GetTicketPriosAsync()
        {
            try
            {
                IEnumerable<TicketPriority> ticketPriorities = await _context.TicketPriorities
                                                                             .ToListAsync();

                return ticketPriorities;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<TicketStatus>> GetTicketStatusesAsync()
        {
            try
            {
                IEnumerable<TicketStatus> ticketStatuses = await _context.TicketStatuses
                                                                         .ToListAsync();

                return ticketStatuses;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<TicketType>> GetTicketTypesAsync()
        {
            try
            {
                IEnumerable<TicketType> ticketTypes = await _context.TicketTypes
                                                                    .ToListAsync();

                return ticketTypes;
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion




    }
}
