
namespace EComApi.Services.Services
{
    public interface IRoleService
    {
        Task InitializeRoles();
        Task<bool> AssignAdminRole(string userId);
        Task<bool> IsUserAdmin(string userId);
        Task<bool> AssignRole(string userId, string roleName);
        Task<List<string>> GetUserRoles(string userId);
    }
}
