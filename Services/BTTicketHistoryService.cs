using CSBugTracker.Data;
using CSBugTracker.Models;
using CSBugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CSBugTracker.Services
{
    public class BTTicketHistoryService : IBTTicketHistoryService
    {
        private readonly ApplicationDbContext _context;
        public BTTicketHistoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddHistoryAsync(Ticket? oldTicket, Ticket? newTicket, string? userId)
        {
            try
            {
                if (oldTicket == null && newTicket != null)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        PropertyName = string.Empty,
                        OldValue = string.Empty,
                        NewValue = string.Empty,
                        Created = DataUtility.GetPostGresDate(DateTime.Now),
                        UserId = userId,
                        Description = "New Ticket Created"
                    };

                    try
                    {
                        await _context.TicketHistories.AddAsync(history);
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception)
                    {

                        throw;
                    }


                }
                else if (oldTicket != null && newTicket != null)
                {

                    // Check Ticket Title
                    if (oldTicket.Title != newTicket.Title)
                    {
                        TicketHistory history = new()
                        {
                            TicketId = newTicket.Id,
                            PropertyName = "Title",
                            OldValue = oldTicket.Title,
                            NewValue = newTicket.Title,
                            Created = DataUtility.GetPostGresDate(DateTime.Now),
                            UserId = userId,
                            Description = $"Changed Ticket Title from '{oldTicket.Title}' to '{newTicket.Title}'"
                        };

                        await _context.TicketHistories.AddAsync(history);
                    }

                    // Check Ticket Descriptiom
                    if (oldTicket.Description != newTicket.Description)
                    {
                        TicketHistory history = new()
                        {
                            TicketId = newTicket.Id,
                            PropertyName = "Description",
                            OldValue = oldTicket.Description,
                            NewValue = newTicket.Description,
                            Created = DataUtility.GetPostGresDate(DateTime.Now),
                            UserId = userId,
                            Description = $"Changed Ticket Description from '{oldTicket.Description}' to '{newTicket.Description}'"
                        };

                        await _context.TicketHistories.AddAsync(history);
                    }

                    // Check Ticket Priority
                    if (oldTicket.TicketPriorityId != newTicket.TicketPriorityId)
                    {
                        TicketHistory history = new()
                        {
                            TicketId = newTicket.Id,
                            PropertyName = "TicketPriority",
                            OldValue = oldTicket.TicketPriority?.Name,
                            NewValue = newTicket.TicketPriority?.Name,
                            Created = DataUtility.GetPostGresDate(DateTime.Now),
                            UserId = userId,
                            Description = $"Changed Ticket Priority from '{oldTicket.TicketPriority?.Name}' to '{newTicket.TicketPriority?.Name}'"
                        };

                        await _context.TicketHistories.AddAsync(history);
                    }

                    // Check Ticket Type
                    if (oldTicket.TicketTypeId != newTicket.TicketTypeId)
                    {
                        TicketHistory history = new()
                        {
                            TicketId = newTicket.Id,
                            PropertyName = "TicketType",
                            OldValue = oldTicket.TicketType?.Name,
                            NewValue = newTicket.TicketType?.Name,
                            Created = DataUtility.GetPostGresDate(DateTime.Now),
                            UserId = userId,
                            Description = $"Changed Ticket Type from '{oldTicket.TicketType?.Name}' to '{newTicket.TicketType?.Name}'"
                        };

                        await _context.TicketHistories.AddAsync(history);
                    }

                    // Check Ticket Status
                    if (oldTicket.TicketStatusId != newTicket.TicketStatusId)
                    {
                        TicketHistory history = new()
                        {
                            TicketId = newTicket.Id,
                            PropertyName = "TicketStatus",
                            OldValue = oldTicket.TicketStatus?.Name,
                            NewValue = newTicket.TicketStatus?.Name,
                            Created = DataUtility.GetPostGresDate(DateTime.Now),
                            UserId = userId,
                            Description = $"Changed Ticket Status from '{oldTicket.TicketStatus?.Name}' to '{newTicket.TicketStatus?.Name}'"
                        };

                        await _context.TicketHistories.AddAsync(history);
                    }

                    // Check Ticket Developer
                    if (oldTicket.DeveloperUserId != newTicket.DeveloperUserId)
                    {
                        TicketHistory history = new()
                        {
                            TicketId = newTicket.Id,
                            PropertyName = "DeveloperUser",
                            OldValue = oldTicket.DeveloperUser?.FullName ?? "Unassigned",
                            NewValue = newTicket.DeveloperUser?.FullName,
                            Created = DataUtility.GetPostGresDate(DateTime.Now),
                            UserId = userId,
                            Description = $"Changed Ticket Developer from '{oldTicket.DeveloperUser?.FullName}' to '{newTicket.DeveloperUser?.FullName}'"
                        };

                        await _context.TicketHistories.AddAsync(history);
                    }


                    try
                    {
                        // Save the TicketHistory DBSet to the database
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception)
                    {

                        throw;
                    }

                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task AddHistoryAsync(int? ticketId, string? model, string? userId)
        {
            try
            {
                Ticket? ticket = await _context.Tickets.FindAsync(ticketId);
                string description = model!.ToLower().Replace("ticket", "");
                description = $"New {description} added to ticket: {ticket?.Title}";

                if (ticket != null)
                {
                    TicketHistory history = new()
                    {
                        TicketId = ticket.Id,
                        PropertyName = model,
                        OldValue = string.Empty,
                        NewValue = string.Empty,
                        Created = DataUtility.GetPostGresDate(DateTime.Now),
                        UserId = userId,
                        Description = description
                    };

                    await _context.TicketHistories.AddAsync(history);
                    await _context.SaveChangesAsync();              
                }



            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<List<TicketHistory>> GetCompanyTicketsHistoriesAsync(int? companyId)
        {
            //List<TicketHistory> histories = await _context.TicketHistories.ToList();
            //return histories;
            throw new NotImplementedException();
        }

        public Task<List<TicketHistory>> GetProjectTicketsHistoriesAsync(int? projectId, int? companyId)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<TicketHistory>> GetRecentTicketHistoryAsync(int? ticketId)
        {
            try
            {
                IEnumerable<TicketHistory> history = await _context.TicketHistories
                                                                   .Where(a => a.TicketId == ticketId)
                                                                   .Include(a => a.User)
                                                                   .Include(a => a.Ticket)
                                                                   .ToListAsync();

                return history.OrderByDescending(a => a.Created);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<TicketHistory>> GetCompanyTicketHistoryAsync(int? companyId)
        {
            try
            {
                IEnumerable<TicketHistory> history = await _context.TicketHistories
                                                                   .Include(a => a.User)
                                                                   .Include(a => a.Ticket).ThenInclude(t => t!.Project)
                                                                   .Where(a => a.Ticket!.Project!.CompanyId == companyId)
                                                                   .ToListAsync();

                return history.OrderByDescending(a => a.Created);
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}
