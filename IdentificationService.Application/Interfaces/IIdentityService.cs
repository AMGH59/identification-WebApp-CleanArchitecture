using IdentificationService.Application.Models;
using IdentificationService.Domain.Entities;

namespace IdentificationService.Application.Interfaces
{
    public interface IIdentityService
    {
        Task<ApplicationUser> CreateUserAsync(string userName, string password, List<string> roles);
        Task<ApplicationRole> CreateRoleAsync(string roleName);
        Task<bool> AssignUserToRole(string userName, IList<string> roles);
        Task<string> LoginUserAsync(string userName, string password);

    }
}
