using CSBugTracker.Data;
using CSBugTracker.Models;
using CSBugTracker.Models.Enums;
using CSBugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace CSBugTracker.Services
{
	public class BTNotificationService : IBTNotificationService
	{

		private readonly ApplicationDbContext _context;
		private readonly IBTRolesService _rolesService;
		private readonly IEmailSender _emailService;
		public BTNotificationService(ApplicationDbContext context,
									 IBTRolesService rolesService,
									 IEmailSender emailService)
		{
			_context = context;
			_rolesService = rolesService;
			_emailService = emailService;
		}


		public async Task AddNotificationAsync(Notification? notification)
		{
			try
			{
				if (notification != null)
				{
					await _context.AddAsync(notification);
					await _context.SaveChangesAsync();
				}
			}
			catch (Exception)
			{

				throw;
			}
		}


		public async Task AdminNotificationAsync(Notification? notification, int? companyId)
		{
			try
			{
				if (notification != null)
				{
					IEnumerable<string> adminIds = (await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Admin), companyId)).Select(u => u.Id);

					foreach (string adminId in adminIds)
					{
						// This allows the database to set the primary key so we send a different notification to each admin
						// only works with Add because there is no 0 to be found if we used Update
						// database creates a unique ID for each notification added
						notification.Id = 0;
						notification.RecipientId = adminId;

						await _context.AddAsync(notification);
					}

					await _context.SaveChangesAsync();
				}
			}
			catch (Exception)
			{

				throw;
			}
		}

		public async Task<List<Notification>> GetNotificationsByUserIdAsync(string? userId)
		{
			try
			{
				List<Notification> notifications = new();

				if (!string.IsNullOrEmpty(userId))
				{

					notifications = await _context.Notifications
												  .Where(n => n.RecipientId == userId || n.SenderId == userId)
												  .Include(n => n.Recipient)
												  .Include(n => n.Sender)
												  .ToListAsync();

				}

				return notifications;

			}
			catch (Exception)
			{

				throw;
			}
		}

		public async Task<bool> SendAdminEmailNotificationAsync(Notification? notification, string? emailSubject, int? companyId)
		{
			try
			{
				if (notification != null)
				{
					IEnumerable<string> adminEmails = (await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Admin), companyId)).Select(u => u.Email)!;

					foreach (string adminEmail in adminEmails)
					{
						await _emailService.SendEmailAsync(adminEmail, emailSubject!, notification.Message!);
					}

					return true;
				}

				return false;

			}
			catch (Exception)
			{

				throw;
			}
		}


		public async Task<bool> SendEmailNotificationAsync(Notification? notification, string? emailSubject)
		{
			try
			{
				if (notification != null)
				{
					BTUser? user = await _context.Users.FirstOrDefaultAsync(u => u.Id == notification.RecipientId);

					string? userEmail = user?.Email;

					if (userEmail != null)
					{
						await _emailService.SendEmailAsync(userEmail, emailSubject!, notification.Message!);

						return true;
					}
				}

				return false;

			}
			catch (Exception)
			{

				throw;
			}
		}
	}
}
