using BWBugTracker.Data;
using BWBugTracker.Models;
using BWBugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BWBugTracker.Services
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
                Company? company = await _context.Companies
                                                 .Include(c => c.Members)
                                                 .Include(c => c.Projects)
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
                List<BTUser>? members = new();

                members = await _context.Users
                                        .Where(u => u.CompanyId == companyId)
                                        .ToListAsync();

                return members;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<BTUser> GetMemberAsync(string? userId, int? companyId)
        {
            BTUser? member = await _context.Users
                                           .FirstOrDefaultAsync(u => u.Id == userId && u.CompanyId == companyId);

            return member!;
        }
    }
}
