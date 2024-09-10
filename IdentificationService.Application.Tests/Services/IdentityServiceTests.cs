using IdentificationService.Domain.Entities;
using IdentificationService.Domain.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace IdentificationService.Application.Tests.Services
{
    public class IdentityServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<RoleManager<ApplicationRole>> _mockRoleManager;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<IdentityService>> _mockLogger;
        private readonly IdentityService _identityService;

        public IdentityServiceTests()
        {
            _mockUserManager = MockUserManager();
            _mockRoleManager = MockRoleManager();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<IdentityService>>();
            _identityService = new IdentityService(_mockUserManager.Object, _mockRoleManager.Object, _mockConfiguration.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldCreateUser_WhenValidInput()
        {
            // Arrange
            var userName = "testuser";
            var password = "Password123!";
            var roles = new List<string> { "Admin" };

            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), password))
                .ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(um => um.AddToRolesAsync(It.IsAny<ApplicationUser>(), roles))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _identityService.CreateUserAsync(userName, password, roles);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userName, result.UserName);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldThrowValidationException_WhenUserCreationFails()
        {
            // Arrange
            var userName = "testuser";
            var password = "Password123!";
            var roles = new List<string> { "Admin" };

            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), password))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "User creation failed." }));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => _identityService.CreateUserAsync(userName, password, roles));
            Assert.Equal("User creation failed.", exception.Message);
        }

        [Fact]
        public async Task CreateRoleAsync_ShouldCreateRole_WhenValidInput()
        {
            // Arrange
            var roleName = "Admin";

            _mockRoleManager.Setup(rm => rm.CreateAsync(It.IsAny<ApplicationRole>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _identityService.CreateRoleAsync(roleName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(roleName, result.Name);
        }

        [Fact]
        public async Task CreateRoleAsync_ShouldThrowException_WhenRoleCreationFails()
        {
            // Arrange
            var roleName = "Admin";

            _mockRoleManager.Setup(rm => rm.CreateAsync(It.IsAny<ApplicationRole>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Role creation failed." }));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _identityService.CreateRoleAsync(roleName));
            Assert.Equal("An error occurred during role creation.", exception.Message);
        }

        private Mock<UserManager<ApplicationUser>> MockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
        }

        private Mock<RoleManager<ApplicationRole>> MockRoleManager()
        {
            var store = new Mock<IRoleStore<ApplicationRole>>();
            return new Mock<RoleManager<ApplicationRole>>(store.Object, null, null, null, null);
        }
    }
}
