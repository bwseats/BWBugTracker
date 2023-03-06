using BWBugTracker.Models;

namespace BWBugTracker.Services.Interfaces
{
    public interface IBTProjectService
    {
        #region

        public Task AddProjectAsync(Project project);
        public Task<Project> GetProjectAsync(int id, int companyId);
        public Task<IEnumerable<Project>> GetProjectsAsync(int companyId);
        public Task UpdateProjectAsync(Project project);
        public Task ArchiveProjectAsync(Project project);

        #endregion

        #region Extended Methods

        public Task<IEnumerable<ProjectPriority>> GetProjectPrioritiesAsync();

        #endregion
    }
}
