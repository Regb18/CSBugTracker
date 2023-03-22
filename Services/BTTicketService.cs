using CSBugTracker.Data;
using CSBugTracker.Models;
using CSBugTracker.Models.Enums;
using CSBugTracker.Services.Interfaces;
using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
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
        public async Task<Ticket> GetTicketAsync(int? ticketId, int? companyId)
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
											   .FirstOrDefaultAsync(t => t.Id == ticketId && t.Project!.CompanyId == companyId);

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

        public async Task<IEnumerable<Ticket>> GetTicketsAsync(string? userId,int? companyId)
        {
            try
            {
                BTUser? user = await _context.Users.FindAsync(userId);

                IEnumerable<Ticket> tickets = await _context.Tickets
                                                             .Include(t => t.DeveloperUser)
                                                             .Include(t => t.Project)
                                                             .Include(t => t.SubmitterUser)
                                                             .Include(t => t.TicketPriority)
                                                             .Include(t => t.TicketStatus)
                                                             .Include(t => t.TicketType)
                                                             .Where(t => t.Project!.CompanyId == companyId && t.Archived == false && t.ArchivedByProject == false)
                                                             .ToListAsync();

                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }


        public async Task<IEnumerable<Ticket>> GetTicketsbyProjectsAsync(int? companyId, int? projectId)
        {
            try
            {
                IEnumerable<Ticket> tickets = await _context.Tickets
                                                             .Include(t => t.DeveloperUser)
                                                             .Include(t => t.Project)
                                                             .Include(t => t.SubmitterUser)
                                                             .Include(t => t.TicketPriority)
                                                             .Include(t => t.TicketStatus)
                                                             .Include(t => t.TicketType)
                                                             .Where(t => t.ProjectId == projectId && t.Project!.CompanyId == companyId && t.Archived == false && t.ArchivedByProject == false)
                                                             .ToListAsync();

                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<Ticket>> GetTicketsbyUserAsync(string? userId)
        {
            try
            {
                IEnumerable<Ticket> tickets = await _context.Tickets
                                                             .Include(t => t.DeveloperUser)
                                                             .Include(t => t.Project)
                                                             .Include(t => t.SubmitterUser)
                                                             .Include(t => t.TicketPriority)
                                                             .Include(t => t.TicketStatus)
                                                             .Include(t => t.TicketType)
                                                             .Where(t => (t.SubmitterUserId == userId || t.DeveloperUserId == userId) && t.Archived == false && t.ArchivedByProject == false)
                                                             .ToListAsync();

                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }


        public async Task<IEnumerable<Ticket>> GetUnassignedTicketsAsync(int? companyId, string? userId)
        {
            try
            {
                BTUser? user = await _context.Users.FindAsync(userId);

                IEnumerable<Ticket> tickets = await _context.Tickets
                                                             .Include(t => t.DeveloperUser)
                                                             .Include(t => t.Project)
                                                             .Include(t => t.SubmitterUser)
                                                             .Include(t => t.TicketPriority)
                                                             .Include(t => t.TicketStatus)
                                                             .Include(t => t.TicketType)
                                                             .Where(t => t.Project!.CompanyId == companyId && t.Archived == false && t.ArchivedByProject == false && t.DeveloperUserId == null)
                                                             .ToListAsync();

                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }


        public async Task<IEnumerable<Ticket>> GetPMUnassignedTicketsAsync(int? companyId, string? userId)
        {
            try
            {
                BTUser? user = await _context.Users.FindAsync(userId);

                //IEnumerable<Project> managerTickets = await _context.Projects
                //                                          .Where(p => p.CompanyId == companyId && p.Members.Contains(user!))
                //                                          .Include(p => p.Company)
                //                                          .Include(p => p.Tickets).ThenInclude(t => t.DeveloperUser)
                //                                          .Include(p => p.Tickets).ThenInclude(t => t.SubmitterUser)
                //                                          .Include(p => p.Tickets).ThenInclude(t => t.TicketPriority)
                //                                          .Include(p => p.Tickets).ThenInclude(t => t.TicketStatus)
                //                                          .Include(p => p.Tickets).ThenInclude(t => t.TicketType)
                //                                          .ToListAsync();

                IEnumerable<Ticket> tickets = await _context.Tickets
                                             .Include(t => t.DeveloperUser)
                                             .Include(t => t.Project)
                                             .Include(t => t.SubmitterUser)
                                             .Include(t => t.TicketPriority)
                                             .Include(t => t.TicketStatus)
                                             .Include(t => t.TicketType)
                                             .Where(t => t.Project!.CompanyId == companyId && t.Archived == false && t.ArchivedByProject == false && t.Project.Members.Contains(user!) && t.DeveloperUserId == null)
                                             .ToListAsync();
                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<Ticket>> GetArchivedTicketsAsync(string? userId, int? companyId)
        {
            try
            {
                BTUser? user = await _context.Users.FindAsync(userId);

                IEnumerable<Ticket> tickets = await _context.Tickets
                                                             .Include(t => t.DeveloperUser)
                                                             .Include(t => t.Project)
                                                             .Include(t => t.SubmitterUser)
                                                             .Include(t => t.TicketPriority)
                                                             .Include(t => t.TicketStatus)
                                                             .Include(t => t.TicketType)
                                                             .Where(t => t.Project!.CompanyId == companyId && (t.Archived == true || t.ArchivedByProject == true))
                                                             .ToListAsync();

                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Ticket> GetTicketAsNoTrackingAsync(int? ticketId, int? companyId)
        {
            Ticket? ticket = await _context.Tickets.Include(t => t.Project)
                                                     .ThenInclude(p => p!.Company)
                                                   .Include(t => t.DeveloperUser)
                                                   .Include(t => t.SubmitterUser)
                                                   .Include(t => t.TicketPriority)
                                                   .Include(t => t.TicketStatus)
                                                   .Include(t => t.TicketType)
                                                   .Include(t => t.TicketHistory)
                                                   .Include(t => t.TicketComments)
                                                   .Include(t => t.TicketAttachments)
                                                   .AsNoTracking()
                                                   .FirstOrDefaultAsync(t => t.Id == ticketId && t.Project!.CompanyId == companyId && t.Archived == false);

            return ticket!;
        }

        #endregion


        #region Get Tickets Developing and Tickets Submitted

        public async Task<int> GetTicketsSubmittedCompanyAsync(string? userId, int? companyId)
        {
            try
            {
                IEnumerable<Ticket> tickets = await _context.Tickets
                                                             .Include(t => t.DeveloperUser)
                                                             .Include(t => t.Project)
                                                             .Include(t => t.SubmitterUser)
                                                             .Include(t => t.TicketPriority)
                                                             .Include(t => t.TicketStatus)
                                                             .Include(t => t.TicketType)
                                                             .Where(t => t.SubmitterUserId == userId && t.Project!.CompanyId == companyId && t.Archived == false && t.ArchivedByProject == false)
                                                             .ToListAsync();
                int numberOfTickets = tickets.Count();

                return numberOfTickets;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<int> GetTicketsDevelopingCompanyAsync(string? userId, int? companyId)
        {
            try
            {
                IEnumerable<Ticket> tickets = await _context.Tickets
                                                             .Include(t => t.DeveloperUser)
                                                             .Include(t => t.Project)
                                                             .Include(t => t.SubmitterUser)
                                                             .Include(t => t.TicketPriority)
                                                             .Include(t => t.TicketStatus)
                                                             .Include(t => t.TicketType)
                                                             .Where(t => t.DeveloperUserId == userId && t.Project!.CompanyId == companyId && t.Archived == false && t.ArchivedByProject == false)
                                                             .ToListAsync();
                int numberOfTickets = tickets.Count();

                return numberOfTickets;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<int> GetTicketsSubmittedProjectAsync(string? userId, int? projectId)
        {
            try
            {
                IEnumerable<Ticket> tickets = await _context.Tickets
                                                             .Include(t => t.DeveloperUser)
                                                             .Include(t => t.Project)
                                                             .Include(t => t.SubmitterUser)
                                                             .Include(t => t.TicketPriority)
                                                             .Include(t => t.TicketStatus)
                                                             .Include(t => t.TicketType)
                                                             .Where(t => t.SubmitterUserId == userId && t.ProjectId == projectId && t.Archived == false && t.ArchivedByProject == false)
                                                             .ToListAsync();
                int numberOfTickets = tickets.Count();

                return numberOfTickets;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<int> GetTicketsDevelopingProjectAsync(string? userId, int? projectId)
        {
            try
            {
                IEnumerable<Ticket> tickets = await _context.Tickets
                                                             .Include(t => t.DeveloperUser)
                                                             .Include(t => t.Project)
                                                             .Include(t => t.SubmitterUser)
                                                             .Include(t => t.TicketPriority)
                                                             .Include(t => t.TicketStatus)
                                                             .Include(t => t.TicketType)
                                                             .Where(t => t.DeveloperUserId == userId && t.ProjectId == projectId && t.Archived == false && t.ArchivedByProject == false)
                                                             .ToListAsync();

                int numberOfTickets = tickets.Count();

                return numberOfTickets;
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

        public async Task<IEnumerable<TicketComment>> GetRecentTicketCommentsAsync(int? ticketId)
        {
            try
            {
                IEnumerable<TicketComment> comments = await _context.TicketComments
                                                                          .Where(a => a.TicketId == ticketId)
                                                                          .Include(a => a.User)
                                                                          .Include(a => a.Ticket)
                                                                          .ToListAsync();


                return comments.OrderByDescending(a => a.Created);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<TicketAttachment>> GetRecentTicketAttachmentsAsync(int? ticketId)
        {
            try
            {
                IEnumerable<TicketAttachment> attachments = await _context.TicketAttachments
                                                                          .Where(a => a.TicketId == ticketId)
                                                                          .Include(a=>a.User)
                                                                          .Include(a => a.Ticket)
                                                                          .ToListAsync();

                return attachments.OrderByDescending(a => a.Created);
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


        #region Developer on Ticket
        public async Task<BTUser> GetTicketDeveloperAsync(int? ticketId)
        {
            try
            {
                Ticket? ticket = await _context.Tickets.Include(t => t.DeveloperUser)
                                                       .Include(t => t.Project).ThenInclude(p => p.Members)
                                                       .FirstOrDefaultAsync(t => t.Id == ticketId);

                foreach (BTUser member in ticket!.Project!.Members)
                {
                    if (member.Id == ticket.DeveloperUser?.Id)
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

        public async Task<bool> AddTicketDeveloperAsync(string? userId, int? ticketId, int? companyId)
        {
            try
            {
                Ticket ticket = await GetTicketAsync(ticketId, companyId);

                BTUser? selectedDev = await _context.Users.FindAsync(userId);

                // Add new/Selected Dev
                try
                {
                    ticket.DeveloperUser = selectedDev;
                    await UpdateTicketAsync(ticket);

                    return true;

                }
                catch (Exception)
                {

                    throw;
                }


            }
            catch (Exception)
            {

                throw;
            }
        }


        #endregion

    }
}
