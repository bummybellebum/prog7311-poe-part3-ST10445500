using System.Security.Claims;
using GLMS.Api.Data.Repositories;
using GLMS.Api.Services;
using GLMS.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Moq;

namespace GLMS.Tests.UnitTests.Services
{
    public class AccountServiceTests
    {
        [Fact]
        public void GetCurrentUserId_WithSignedInUser_ReturnsClaimValue()
        {
            // Arrange
            var userRepository = new Mock<IUserRepository>();
            var accessor = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, TestData.UserId)
                    }, "Test"))
                }
            };
            var service = new AccountService(userRepository.Object, accessor);

            // Act
            var result = service.GetCurrentUserId();

            // Assert
            Assert.Equal(TestData.UserId, result);
        }

        [Fact]
        public void GetCurrentUserId_WithoutSignedInUser_ThrowsValidationError()
        {
            // Arrange
            var userRepository = new Mock<IUserRepository>();
            var accessor = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            };
            var service = new AccountService(userRepository.Object, accessor);

            // Act
            var exception = Assert.Throws<InvalidOperationException>(() => service.GetCurrentUserId());

            // Assert
            Assert.Equal("Unable to identify the signed-in user.", exception.Message);
        }
    }
}
