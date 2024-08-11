namespace IdentificationService.Database.Interfaces
{
    internal interface IDataSeeder
    {
        short SequenceNumber { get; }
        void Seed(IApplicationDbContext context);
    }
}
