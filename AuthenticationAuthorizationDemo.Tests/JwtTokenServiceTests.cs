using AuthenticationAuthorizationDemo.Configuration;
using AuthenticationAuthorizationDemo.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace AuthenticationAuthorizationDemo.Tests
{
    public class JwtTokenServiceTests
    {
        // These should be mocks, not the real services, for a Unit Test
        private readonly Mock<UserManager<IdentityUser>> _mockUserManager;
        private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;

        public JwtTokenServiceTests()
        {
            // Boilerplate setup for Mocking UserManager
            var store = new Mock<IUserStore<IdentityUser>>();
            _mockUserManager = new Mock<UserManager<IdentityUser>>(store.Object, null, null, null, null, null, null, null, null);

            var roleStore = new Mock<IRoleStore<IdentityRole>>();
            _mockRoleManager = new Mock<RoleManager<IdentityRole>>(roleStore.Object, null, null, null, null);
        }

        [Fact]
        public async Task GenerateToken_ValidUser_ReturnsValidJwt()
        {
            // 1. Arrange settings
            var settings = new JwtSettings
            {
                Issuer = "test",
                Audience = "test",
                SecretKey = "supersecretkey12345678901234567890",
                AccessTokenExpirationMinutes = 15
            };

            var mockOptions = new Mock<IOptions<JwtSettings>>();
            mockOptions.Setup(o => o.Value).Returns(settings);

            // 2. Setup User and Mock behavior
            var user = new IdentityUser { Id = "1", Email = "test@test.com" };

            // Mocking GetRolesAsync to return an empty list instead of null
            // This prevents the NullReferenceException when the service iterates over roles
            _mockUserManager.Setup(um => um.GetRolesAsync(It.IsAny<IdentityUser>()))
                .ReturnsAsync(new List<string>());

            var service = new JwtTokenService(mockOptions.Object, _mockUserManager.Object, _mockRoleManager.Object);

            // 3. Act
            var token = await service.GenerateToken(user);

            // 4. Assert
            Assert.NotEmpty(token);
        }
    }
}