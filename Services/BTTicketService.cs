using CSBugTracker.Data;
using CSBugTracker.Models;
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


		public async Task AddCommentAsync(TicketComment comment, int ticketId)
		{
			Ticket? ticket = await _context.Tickets
										   .Include(c => c.TicketComments) // Eager Load
										   .FirstOrDefaultAsync(c => c.Id == ticketId);

			try
			{
				_context.Add(comment);
				ticket!.TicketComments.Add(comment);

				await _context.SaveChangesAsync();
			}
			catch (Exception)
			{

				throw;
			}
		}

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
											   .Include(t => t.TicketAttachments)
											   .Include(t => t.TicketHistory)
											   .FirstOrDefaultAsync(m => m.Id == ticketId);

				return ticket!;
			}
			catch (Exception)
			{

				throw;
			}
		}
	}
}
