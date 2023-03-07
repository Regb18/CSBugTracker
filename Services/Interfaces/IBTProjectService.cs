using CSBugTracker.Models;

namespace CSBugTracker.Services.Interfaces
{
	public interface IBTProjectService
	{
        // Crud Services

        // Add Project

        // Update Project

        // Get Project
        public Task<Project> GetProjectAsync(int? projectId, int companyId);

        // Delete(Archive) Project





        // Get Projectsss (recent projects, users projects, priority, size(number of members), order of completion(least tickets unresolved))
        public Task<BTUser> GetMyProjectsAsync(string userId);

        public Task<IEnumerable<Project>> GetProjectsAsync(int companyId);




        // Additional Services

        // Add multiple members
        public Task AddProjectToMembersAsync(IEnumerable<string> memberIds, int projectId);
        public Task<bool> IsTagOnProjectAsync(string memberId, int projectId);
        public Task RemoveAllProjectMembersAsync(int projectId);
        // Search Projects



        // Get Members

        public Task<IEnumerable<BTUser>> GetMembersAsync(int companyId);


    }
}
