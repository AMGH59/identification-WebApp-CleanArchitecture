namespace IdentificationService.Application.Models
{
    public record UserModel(string Username, string Password, List<string> Roles);
}
