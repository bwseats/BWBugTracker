using BWBugTracker.Data;
using BWBugTracker.Models;
using BWBugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;

namespace BWBugTracker.Services
{
    public class BTProjectService : IBTProjectService
    {
        private readonly ApplicationDbContext _context;

        public BTProjectService(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Projects CRUD

        public Task AddProjectAsync(Project project)
        {
            throw new NotImplementedException();
        }

        public Task ArchiveProjectAsync(Project project)
        {
            throw new NotImplementedException();
        }

        public async Task<Project> GetProjectAsync(int id, int companyId)
        {
            try
            {
                Project? project = await _context.Projects
                                                 .Include(p => p.Company)
                                                 .Include(p => p.Members)
                                                 .Include(p => p.ProjectPriority)
                                                 .Include(p => p.Tickets)
                                                     .ThenInclude(t => t.DeveloperUser)
                                                 .Include(p => p.Tickets)
                                                     .ThenInclude(t => t.SubmitterUser)
                                                 .FirstOrDefaultAsync(p => p.CompanyId == companyId);

                return project!;
            }
            catch (Exception)
            {

                throw;
            }
        }
        
        public async Task<IEnumerable<Project>> GetProjectsAsync(int companyId)
        {
            try
            {
                IEnumerable<Project> projects = await _context.Projects
                                                              .Where(p => p.Archived == false && p.CompanyId == companyId)
                                                              .Include(p => p.Members)
                                                              .Include(p => p.ProjectPriority)
                                                              .Include(p => p.Tickets)
                                                              .ToListAsync();

                return projects;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task UpdateProjectAsync(Project project)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Extended Methods

        public async Task<IEnumerable<ProjectPriority>> GetProjectPrioritiesAsync()
        {
            try
            {
                IEnumerable<ProjectPriority> priorities = await _context.ProjectPriorities.ToListAsync();

                return priorities;
            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion
    }
}
