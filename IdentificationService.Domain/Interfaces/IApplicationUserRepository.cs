using IdentificationService.Domain.Entities;

namespace IdentificationService.Domain.Interfaces
{
    public interface IApplicationUserRepository
    {
        Task<IEnumerable<ApplicationUser>> GetAll(CancellationToken cancellationToken = default);
    }
}
