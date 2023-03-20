using CSBugTracker.Models;

namespace CSBugTracker.Services.Interfaces
{
	public interface IBTProjectService
	{
        
        #region CRUD methods
        public Task<Project> GetProjectAsync(int? projectId, int? companyId);
        public Task AddProjectAsync(Project project);
        public Task UpdateProjectAsync(Project project);
        public Task DeleteProjectAsync(Project project);
        #endregion


        public Task<bool> AddMemberToProjectAsync(BTUser? member, int? projectId);
        public Task<bool> AddProjectManagerAsync(string? userId, int? projectId);
        public Task<BTUser> GetProjectManagerAsync(int? projectId);


        public Task RemoveProjectManagerAsync(int? projectId);
        public Task<bool> RemoveMemberFromProjectAsync(BTUser? member, int? projectId);


        // Get Projects (recent projects, users projects, priority, size(number of members), order of completion(least tickets unresolved))
        #region Get Projects Methods
        public Task<IEnumerable<Project>> GetMyProjectsAsync(string? userId, int? companyId);
        public Task<IEnumerable<Project>> GetProjectsAsync(int? companyId);
        public Task<IEnumerable<Project>> GetProjectsAsync();
        public Task<IEnumerable<Project>> GetArchivedProjectsAsync(int? companyId);
        #endregion



        // Additional Services
        #region Add Multiple Members
        public Task AddProjectToMembersAsync(IEnumerable<string> memberIds, int? projectId, int? companyId);
        public Task<bool> IsMemberOnProjectAsync(string memberId, int projectId);
        public Task RemoveAllProjectMembersAsync(int? projectId, int? companyId);
        #endregion

        #region Projects Navigation Properties
        public Task<IEnumerable<ProjectPriority>> GetProjectPriosAsync();
        public Task<List<BTUser>> GetProjectDevelopersAsync(int? projectId);
        #endregion


        public Task<int> GetArchivedProjectTicketCountAsync(int? projectId, int? companyId);
        public Task<int> GetResolvedProjectTicketCountAsync(int? projectId, int? companyId);
        public Task<int> GetDevelopmentProjectTicketCountAsync(int? projectId, int? companyId);
        public Task<int> GetUnassignedProjectTicketCountAsync(int? projectId, int? companyId);
    }
}
