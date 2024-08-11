using Microsoft.AspNetCore.Identity;

namespace IdentificationService.Domain.Entities
{
    public class ApplicationRole : IdentityRole<Guid>
    {
        public ApplicationRole(string name)
        {
            Name = name;
            NormalizedName = name.ToUpper();
        }
    }
}
