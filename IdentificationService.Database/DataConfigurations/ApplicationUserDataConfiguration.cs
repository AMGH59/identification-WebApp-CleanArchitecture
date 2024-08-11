using IdentificationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentificationService.Database.DataConfigurations
{
    internal class ApplicationUserDataConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Ignore(x => x.Email);
            builder.Ignore(x => x.NormalizedEmail);
            builder.Ignore(x => x.EmailConfirmed);
            builder.Ignore(x => x.PhoneNumberConfirmed);
            builder.Ignore(x => x.PhoneNumber);
        }
    }
}
