using IdentificationService.API.Controller;
using IdentificationService.API.DTO;
using IdentificationService.Application.Constants;
using IdentificationService.Application.Interfaces;
using IdentificationService.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace IdentificationService.API.Tests.Controller
{
    public class AccountControllerTests
    {
        private readonly Mock<IIdentityService> _mockIdentityService;
        private readonly AccountController _controller;

        public AccountControllerTests()
        {
            _mockIdentityService = new Mock<IIdentityService>();
            _controller = new AccountController(_mockIdentityService.Object);
        }

        [Fact]
        public async Task SignIn_ReturnsOkResult_WithToken()
        {
            // Arrange
            var loginDTO = new LoginDTO { Username = "testuser", Password = "password" };
            var token = "testtoken";
            _mockIdentityService.Setup(service => service.LoginUserAsync(loginDTO.Username, loginDTO.Password))
                .ReturnsAsync(token);

            // Act
            var result = await _controller.SignIn(loginDTO);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var returnValue = okResult.Value.GetType().GetProperty("token")?.GetValue(okResult.Value);
            Assert.NotNull(returnValue);
            Assert.IsType<string>(returnValue);
            Assert.Equal(token, returnValue);
        }


        [Fact]
        public async Task SignIn_ReturnsBadRequest_OnException()
        {
            // Arrange
            var loginDTO = new LoginDTO { Username = "testuser", Password = "password" };
            _mockIdentityService.Setup(service => service.LoginUserAsync(loginDTO.Username, loginDTO.Password))
                .ThrowsAsync(new Exception("Invalid credentials"));

            // Act
            var result = await _controller.SignIn(loginDTO);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid credentials", badRequestResult.Value);
        }

        [Fact]
        public async Task Create_ReturnsOkResult_WithUser()
        {
            // Arrange
            var userDTO = new UserDTO { Username = "newuser", Password = "password", Roles = new List<string> { RoleTypes.Admin } };
            var createdUser = new ApplicationUser(userDTO.Username);
            _mockIdentityService.Setup(service => service.CreateUserAsync(userDTO.Username, userDTO.Password, userDTO.Roles))
                .ReturnsAsync(createdUser);

            // Act
            var result = await _controller.Create(userDTO);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var returnValue = okResult.Value.GetType().GetProperty("user")?.GetValue(okResult.Value, null);
            Assert.NotNull(returnValue);
            Assert.IsType<ApplicationUser>(returnValue);
            Assert.Equal(createdUser, returnValue);
        }


        [Fact]
        public async Task Create_ReturnsBadRequest_OnException()
        {
            // Arrange
            var userDTO = new UserDTO { Username = "newuser", Password = "password", Roles = new List<string> { RoleTypes.Admin } };
            _mockIdentityService.Setup(service => service.CreateUserAsync(userDTO.Username, userDTO.Password, userDTO.Roles))
                .ThrowsAsync(new Exception("User creation failed"));

            // Act
            var result = await _controller.Create(userDTO);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("User creation failed", badRequestResult.Value);
        }
    }
}
