using CSBugTracker.Data;
using CSBugTracker.Models;
using CSBugTracker.Models.Enums;
using CSBugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CSBugTracker.Services
{
    public class BTCompanyService : IBTCompanyService
    {
        private readonly ApplicationDbContext _context;

        public BTCompanyService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Company> GetCompanyInfoAsync(int? companyId)
        {
            try
            {
                Company? company = await _context.Companies.Include(c => c.Members)
                                                           .Include(c => c.Projects).ThenInclude(p => p.Tickets)
                                                           .Include(c => c.Invites)
                                                           .FirstOrDefaultAsync(c => c.Id == companyId);
                return company!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<BTUser>> GetMembersAsync(int? companyId)
        {
            try
            {
                List<BTUser>? members = await _context.Users
                                                .Where(u => u.CompanyId == companyId)
                                                .Include(p => p.Company)
                                                .Include(p => p.Projects)
                                                .ToListAsync();

                return members;
            }
            catch (Exception)
            {

                throw;
            }


        }
    }
}
