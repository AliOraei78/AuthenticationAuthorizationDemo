using AuthenticationAuthorizationDemo;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace AuthenticationAuthorizationDemo.Tests
{
    public class AccountControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public AccountControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Register_ValidInput_ReturnsOk()
        {
            var payload = new
            {
                email = "testuser@example.com",
                password = "Test@123!",
                confirmPassword = "Test@123!"
            };

            var response = await _client.PostAsJsonAsync("/api/account/register", payload);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsTokens()
        {
            var payload = new
            {
                email = "admin@example.com",
                password = "Admin@123!"
            };

            var response = await _client.PostAsJsonAsync("/api/account/login", payload);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadFromJsonAsync<object>();
            Assert.NotNull(content);
            // Verify that AccessToken and RefreshToken exist in the response
        }

        [Fact]
        public async Task ProtectedEndpoint_WithValidToken_ReturnsOk()
        {
            // First log in and obtain a token
            var loginPayload = new { email = "admin@example.com", password = "Admin@123!" };
            var loginResponse = await _client.PostAsJsonAsync("/api/account/login", loginPayload);

            // Use 'object' instead of 'string' to accommodate the 'ExpiresInMinutes' integer
            var tokens = await loginResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();

            // Extract the token and cast it to string
            // Note: Use "accessToken" (camelCase) as that is the standard JSON default
            var accessToken = tokens["accessToken"].ToString();

            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _client.GetAsync("/api/account/protected");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}