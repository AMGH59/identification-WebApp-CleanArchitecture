using IdentificationService.Application.Interfaces;
using IdentificationService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentificationService.Domain.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IConfiguration _configuration;

        public IdentityService(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        public async Task<ApplicationUser> CreateUserAsync(string userName, string password, List<string> roles)
        {
            var user = new ApplicationUser(userName);

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                throw new ValidationException(result.Errors.ToString());
            }

            var addUserRole = await _userManager.AddToRolesAsync(user, roles);

            if (!addUserRole.Succeeded)
            {
                throw new ValidationException(addUserRole.Errors.ToString());
            }

            return user;
        }

        public async Task<ApplicationRole> CreateRoleAsync(string roleName)
        {
            var role = new ApplicationRole(roleName);

            var result = await _roleManager.CreateAsync(role)
                ?? throw new Exception("An error occurred during role creation.");

            return role;
        }

        public async Task<bool> AssignUserToRole(string userName, IList<string> roles)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == userName)
                ?? throw new Exception("User not found.");

            var result = await _userManager.AddToRolesAsync(user, roles);

            return result.Succeeded;
        }

        public async Task<string> LoginUserAsync(string userName, string password)
        {
            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.UserName == userName)
                ?? throw new ArgumentNullException("An error occurred during credentials checking.");

            if (!await CheckPasswordAsync(user, password))
                throw new ValidationException("An error occurred during credentials checking.");

            return await GenerateToken(user);
        }

        private async Task<string> GenerateToken(ApplicationUser user)
        {
            var claims = await GenerateClaims(user);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credantials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credantials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<IEnumerable<Claim>> GenerateClaims(ApplicationUser user)
        {
            var claims = new List<Claim>()
                {
                    new(JwtRegisteredClaimNames.Sub, user.UserName),
                    new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

            foreach (var role in await _userManager.GetRolesAsync(user))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return claims;
        }

        private async Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
        {
            return await _userManager.CheckPasswordAsync(user, password);
        }

    }
}
