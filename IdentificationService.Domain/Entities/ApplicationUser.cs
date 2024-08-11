using Microsoft.AspNetCore.Identity;

namespace IdentificationService.Domain.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public ApplicationUser(string userName)
        {
            UserName = userName;
            NormalizedUserName = userName.ToUpper();
        }
    }
}
