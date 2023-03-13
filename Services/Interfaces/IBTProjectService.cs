using BWBugTracker.Models;

namespace BWBugTracker.Services.Interfaces
{
    public interface IBTProjectService
    {
        #region

        public Task AddProjectAsync(Project project);
        public Task<Project> GetProjectAsync(int projectId, int companyId);
        public Task<IEnumerable<Project>> GetProjectsAsync(int companyId);
        public Task UpdateProjectAsync(Project project);
        public Task ArchiveProjectAsync(Project project);
        public Task<List<Project>> GetAllProjectsByCompanyIdAsync(int companyId);
        public Task<List<Project>> GetArchivedProjectsByCompanyIdAsync(int companyId);

        #endregion

        #region Extended Methods


        public Task AddMembersToProjectAsync(IEnumerable<string> userIds, int? projectId, int? companyId);
        
        public Task RemoveMembersFromProjectAsync(int? projectId, int? companyId);

        public Task<bool> AddMemberToProjectAsync(BTUser? member, int? projectId);

        public Task<bool> AddProjectManagerAsync(string? userId, int? projectId);

        public Task<Project> GetProjectByIdAsync(int? projectId, int? companyId);

        public Task<BTUser> GetProjectManagerAsync(int? projectId);



        public Task RemoveProjectManagerAsync(int? projectId);

        public Task<bool> RemoveMemberFromProjectAsync(BTUser? member, int? projectId);



        public Task<IEnumerable<ProjectPriority>> GetProjectPrioritiesAsync();

        public bool ProjectExists(int? projectId);

        #endregion
    }
}
