using BWBugTracker.Data;
using BWBugTracker.Models;
using BWBugTracker.Models.Enums;
using BWBugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;

namespace BWBugTracker.Services
{
    public class BTProjectService : IBTProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTRolesService _rolesService;

        public BTProjectService(ApplicationDbContext context, IBTRolesService rolesService)
        {
            _context = context;
            _rolesService = rolesService;
        }

        #region Projects CRUD

        public async Task AddProjectAsync(Project project)
        {
            try
            {
                _context.Add(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task ArchiveProjectAsync(Project project)
        {
            try
            {
                if (project != null)
                {
                    project.Archived = true;
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Project> GetProjectAsync(int projectId, int companyId)
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

        public async Task UpdateProjectAsync(Project project)
        {
            try
            {
                _context.Update(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
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

        public bool ProjectExists(int projectId)
        {
            try
            {
                return (_context.Projects?.Any(p => p.Id == projectId)).GetValueOrDefault();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<List<Project>> GetAllProjectsByCompanyIdAsync(int? companyId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Project>> GetArchivedProjectsByCompanyIdAsync(int? companyId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> AddMemberToProjectAsync(BTUser? member, int? projectId)
        {
            try
            {
                Project? project = await GetProjectByIdAsync(projectId, member!.CompanyId);

                bool IsOnProject = project.Members.Any(m => m.Id == member.Id);

                if (!IsOnProject)
                {
                    project.Members.Add(member);
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> AddProjectManagerAsync(string? userId, int? projectId)
        {
            try
            {
                BTUser? currentPM = await GetProjectManagerAsync(projectId);
                BTUser? selectedPM = await _context.Users.FindAsync(userId);

                if (currentPM != null)
                {
                    await RemoveProjectManagerAsync(projectId);
                }
                try
                {
                    await AddMemberToProjectAsync(selectedPM, projectId);
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

        public async Task<Project> GetProjectByIdAsync(int? projectId, int? companyId)
        {
            try
            {
                Project? project = await _context.Projects
                                                 .Where(p => p.CompanyId == companyId)
                                                 .Include(p => p.Members)
                                                 .Include(p => p.ProjectPriority)
                                                 .Include(p => p.Tickets)
                                                     .ThenInclude(t => t.DeveloperUser)
                                                 .Include(p => p.Tickets)
                                                     .ThenInclude(t => t.SubmitterUser)
                                                 .FirstOrDefaultAsync(p => p.Id == projectId);

                return project!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<BTUser> GetProjectManagerAsync(int? projectId)
        {
            try
            {
                Project? project = await _context.Projects
                                                .Include(p => p.Members)
                                                .FirstOrDefaultAsync(p => p.Id == projectId);

                foreach (BTUser member in project!.Members)
                {
                    if (await _rolesService.IsUserInRoleAsync(member, nameof(BTRoles.ProjectManager)))
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
        
        public async Task<bool> RemoveMemberFromProjectAsync(BTUser? member, int? projectId)
        {
            try
            {
                Project? project = await GetProjectByIdAsync(projectId, member!.CompanyId);

                bool IsOnProject = project.Members.Any(m => m.Id == member.Id);

                if (IsOnProject)
                {
                    project.Members.Remove(member);
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RemoveProjectManagerAsync(int? projectId)
        {
            try
            {
                Project? project = await _context.Projects
                                                .Include(p => p.Members)
                                                .FirstOrDefaultAsync(p => p.Id == projectId);

                foreach (BTUser member in project!.Members)
                {
                    if (await _rolesService.IsUserInRoleAsync(member, nameof(BTRoles.ProjectManager)))
                    {
                        await RemoveMemberFromProjectAsync(member, projectId);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

		public Task<List<Project>> GetAllProjectsByCompanyIdAsync(int companyId)
		{
			throw new NotImplementedException();
		}

		public Task<List<Project>> GetArchivedProjectsByCompanyIdAsync(int companyId)
		{
			throw new NotImplementedException();
		}

		public bool ProjectExists(int? projectId)
		{
			throw new NotImplementedException();
		}

        public async Task<bool> AddMembersToProjectAsync(IEnumerable<string> userIds, int? projectId, int? companyId)
        {
            try
            {
                Project? project = await GetProjectByIdAsync(projectId, companyId);

                foreach (string userId in userIds)
                {
                    BTUser? btUser = await _context.Users.FindAsync(userId);

                    //await AddMemberToProjectAsync(btUser, projectId);

                    if (project != null && btUser != null)
                    {
                        bool IsOnProject = project.Members.Any(m => m.Id == userId);

                        if (!IsOnProject)
                        {
                            project.Members.Add(btUser);
                        }
                    }
                }

                await _context.SaveChangesAsync();

            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<bool> RemoveMembersFromProjectAsync(int? projectId, int? companyId)
        {
            throw new NotImplementedException();
        }


        #endregion
    }
}
