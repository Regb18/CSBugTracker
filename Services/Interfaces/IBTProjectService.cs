using CSBugTracker.Models;

namespace CSBugTracker.Services.Interfaces
{
	public interface IBTProjectService
	{
        
        #region CRUD methods
        public Task<Project> GetProjectAsync(int? projectId, int companyId);
        public Task AddProjectAsync(Project project);
        public Task UpdateProjectAsync(Project project);
        public Task DeleteProjectAsync(Project project);
        #endregion


        // Get Projects (recent projects, users projects, priority, size(number of members), order of completion(least tickets unresolved))
        #region Get Projects Methods
        public Task<BTUser> GetMyProjectsAsync(string userId);
        public Task<IEnumerable<Project>> GetProjectsAsync(int companyId);
        public Task<IEnumerable<Project>> GetProjectsAsync();
        #endregion



        // Additional Services
        #region Add Multiple Members
        public Task AddProjectToMembersAsync(IEnumerable<string> memberIds, int projectId);
        public Task<bool> IsTagOnProjectAsync(string memberId, int projectId);
        public Task RemoveAllProjectMembersAsync(int projectId);
        #endregion

        #region Projects Navigation Properties
        public Task<IEnumerable<BTUser>> GetMembersAsync(int companyId);
        public Task<IEnumerable<ProjectPriority>> GetProjectPriosAsync();
        #endregion


        // Search Projects //
    }
}
