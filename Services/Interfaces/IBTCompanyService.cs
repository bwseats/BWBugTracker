using BWBugTracker.Models;

namespace BWBugTracker.Services.Interfaces
{
    public interface IBTCompanyService
    {
        public Task<IEnumerable<Company>> GetRecentCompaniesAsync();
		public Task<Company> GetCompanyInfoAsync(int? companyId);
        public Task<List<BTUser>> GetMembersAsync(int? companyId);
        public Task<BTUser> GetMemberAsync(string? userId, int? companyId);
    }
}
