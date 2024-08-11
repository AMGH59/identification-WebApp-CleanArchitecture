namespace IdentificationService.Domain.Interfaces
{
    public interface IApplicationRepository
    {
        void AddObject(object entity);

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        IApplicationUserRepository ApplicationUserRepository { get; }
        IApplicationRoleRepository ApplicationRoleRepository { get; }
    }
}
