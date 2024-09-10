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
            ValidateUserName(userName);
            ValidatePassword(password);
            ValidateRoles(roles);

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
            ValidateRoleName(roleName);

            var role = new ApplicationRole(roleName);

            var result = await _roleManager.CreateAsync(role);

            if (!result.Succeeded)
            {
                throw new Exception("An error occurred during role creation.");
            }

            return role;
        }

        public async Task<bool> AssignUserToRole(string userName, IList<string> roles)
        {
            ValidateUserName(userName);
            ValidateRoles(roles);

            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == userName)
                ?? throw new Exception("User not found.");

            var result = await _userManager.AddToRolesAsync(user, roles);

            return result.Succeeded;
        }

        public async Task<string> LoginUserAsync(string userName, string password)
        {
            ValidateUserName(userName);
            ValidatePassword(password);

            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.UserName == userName)
                ?? throw new ArgumentNullException("User not found.");

            if (!await CheckPasswordAsync(user, password))
            {
                throw new ValidationException("Invalid credentials.");
            }

            return await GenerateToken(user);
        }

        private async Task<string> GenerateToken(ApplicationUser user)
        {
            var claims = await GenerateClaims(user);

            var key = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("JWT key is not configured.");
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<IEnumerable<Claim>> GenerateClaims(ApplicationUser user)
        {
            ArgumentNullException.ThrowIfNull(user);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.UserName ?? throw new ArgumentNullException(nameof(user.UserName))),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            return claims;
        }

        private async Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
        {
            return await _userManager.CheckPasswordAsync(user, password);
        }

        private void ValidateUserName(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new ValidationException("Invalid credentials.");
            }
        }

        private void ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ValidationException("Invalid credentials.");
            }
        }

        private void ValidateRoles(IEnumerable<string> roles)
        {
            if (roles == null || !roles.Any())
            {
                throw new ValidationException("Roles cannot be empty.");
            }
        }

        private void ValidateRoleName(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ValidationException("Role name cannot be empty.");
            }
        }
    }
}
