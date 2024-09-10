using IdentificationService.Application.Interfaces;
using IdentificationService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<IdentityService> _logger;

        public IdentityService(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, IConfiguration configuration, ILogger<IdentityService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ApplicationUser> CreateUserAsync(string userName, string password, List<string> roles)
        {
            _logger.LogInformation("Creating user with username: {UserName}", userName);

            ValidateUserName(userName);
            ValidatePassword(password);
            ValidateRoles(roles);

            var user = new ApplicationUser(userName);

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Failed to create user: {UserName}. Errors: {Errors}", userName, string.Join(", ", result.Errors.Select(e => e.Description)));
                throw new ValidationException(string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            var addUserRole = await _userManager.AddToRolesAsync(user, roles);

            if (!addUserRole.Succeeded)
            {
                _logger.LogWarning("Failed to add roles to user: {UserName}. Errors: {Errors}", userName, string.Join(", ", addUserRole.Errors.Select(e => e.Description)));
                throw new ValidationException(string.Join(", ", addUserRole.Errors.Select(e => e.Description)));
            }

            _logger.LogInformation("User created successfully: {UserName}", userName);
            return user;
        }

        public async Task<ApplicationRole> CreateRoleAsync(string roleName)
        {
            _logger.LogInformation("Creating role with name: {RoleName}", roleName);

            ValidateRoleName(roleName);

            var role = new ApplicationRole(roleName);

            var result = await _roleManager.CreateAsync(role);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Failed to create role: {RoleName}. Errors: {Errors}", roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                throw new Exception("An error occurred during role creation.");
            }

            _logger.LogInformation("Role created successfully: {RoleName}", roleName);
            return role;
        }

        public async Task<bool> AssignUserToRole(string userName, IList<string> roles)
        {
            _logger.LogInformation("Assigning roles to user: {UserName}", userName);

            ValidateUserName(userName);
            ValidateRoles(roles);

            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == userName)
                ?? throw new Exception("User not found.");

            var result = await _userManager.AddToRolesAsync(user, roles);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Failed to assign roles to user: {UserName}. Errors: {Errors}", userName, string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            _logger.LogInformation("Roles assigned to user: {UserName}", userName);
            return result.Succeeded;
        }

        public async Task<string> LoginUserAsync(string userName, string password)
        {
            _logger.LogInformation("Logging in user with username: {UserName}", userName);

            ValidateUserName(userName);
            ValidatePassword(password);

            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.UserName == userName)
                ?? throw new ArgumentNullException("User not found.");

            if (!await CheckPasswordAsync(user, password))
            {
                _logger.LogWarning("Invalid credentials for user: {UserName}", userName);
                throw new ValidationException("Invalid credentials.");
            }

            var token = await GenerateToken(user);
            _logger.LogInformation("User logged in successfully: {UserName}", userName);
            return token;
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
