using IdentificationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentificationService.Database.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<ApplicationUser> Users { get; }
        DbSet<ApplicationRole> Roles { get; }

        void AddObject(object entity);

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
