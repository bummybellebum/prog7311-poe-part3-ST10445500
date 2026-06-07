using GLMS.Api.Controllers;
using GLMS.Api.DTOs.Auth;
using GLMS.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GLMS.Tests.UnitTests.Controllers
{
    public class AuthControllerTests
    {
        [Fact]
        public async Task Login_WithValidCredentials_ReturnsOk()
        {
            // Arrange
            var authService = new Mock<IAuthService>();
            var dto = new LoginRequestDto
            {
                Email = "admin@glms.local",
                Password = "Password123!"
            };
            authService
                .Setup(service => service.LoginAsync(dto))
                .ReturnsAsync(AuthServiceResult<AuthResponseDto>.Success(new AuthResponseDto
                {
                    UserId = "user-1",
                    Email = dto.Email,
                    Token = "token"
                }));
            var controller = new AuthController(authService.Object);

            // Act
            var result = await controller.Login(dto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var authService = new Mock<IAuthService>();
            var dto = new LoginRequestDto
            {
                Email = "admin@glms.local",
                Password = "wrong"
            };
            authService
                .Setup(service => service.LoginAsync(dto))
                .ReturnsAsync(AuthServiceResult<AuthResponseDto>.Unauthorized("Invalid login attempt."));
            var controller = new AuthController(authService.Object);

            // Act
            var result = await controller.Login(dto);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }
    }
}
