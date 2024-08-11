using IdentificationService.Database.Interfaces;
using IdentificationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace IdentificationService.Database.DataContext
{
    internal class ApplicationContext : DbContext, IApplicationDbContext
    {
        public DbSet<ApplicationUser> Users => Set<ApplicationUser>();

        public DbSet<ApplicationRole> Roles => Set<ApplicationRole>();

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);
        }

        public void AddObject(object entity)
        {
            base.Add(entity);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
