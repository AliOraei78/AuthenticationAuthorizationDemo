using AuthenticationAuthorizationDemo.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AuthenticationAuthorizationDemo.Services
{
    public class JwtTokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly UserManager<IdentityUser> _userManager;     // new
        private readonly RoleManager<IdentityRole> _roleManager;     // new

        public JwtTokenService(
            IOptions<JwtSettings> jwtSettings,
            UserManager<IdentityUser> userManager,   // new injection
            RoleManager<IdentityRole> roleManager)   // new injection
        {
            _jwtSettings = jwtSettings.Value;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<string> GenerateToken(IdentityUser user)  // ← roles parameter removed
        {
            // Get user roles
            var userRoles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new Claim("auth_time", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new Claim("email_verified", user.EmailConfirmed.ToString().ToLowerInvariant()),
        };

            // Add roles as claims (for [Authorize(Roles = "...")])
            foreach (var roleName in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, roleName));

                // Read role claims (permissions)
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    var roleClaims = await _roleManager.GetClaimsAsync(role);

                    foreach (var claim in roleClaims.Where(c => c.Type == "Permission"))
                    {
                        claims.Add(new Claim("Permission", claim.Value));
                    }
                }
            }

            // For age testing (you can later read this from a real database)
            claims.Add(new Claim("DateOfBirth", "1990-01-01"));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public string HashRefreshToken(string token)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(hashBytes);
        }

        public async Task<(string AccessToken, string RefreshToken)> GenerateTokenPair(IdentityUser user)
        {
            var accessToken = await GenerateToken(user);
            var refreshToken = GenerateRefreshToken();

            return (accessToken, refreshToken);
        }
    }
}