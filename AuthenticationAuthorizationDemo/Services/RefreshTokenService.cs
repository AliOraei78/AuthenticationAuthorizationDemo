using AuthenticationAuthorizationDemo.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationAuthorizationDemo.Services
{
    public class RefreshTokenService
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtTokenService _jwtTokenService;

        public RefreshTokenService(ApplicationDbContext context, JwtTokenService jwtTokenService)
        {
            _context = context;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<string> CreateRefreshTokenAsync(string userId, string ipAddress)
        {
            var rawToken = _jwtTokenService.GenerateRefreshToken();
            var hashedToken = _jwtTokenService.HashRefreshToken(rawToken);

            var refreshTokenEntity = new RefreshToken
            {
                UserId = userId,
                TokenHash = hashedToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),  // or read from config
                CreatedAt = DateTime.UtcNow,
                RevokedByIp = ipAddress
            };

            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            return rawToken;  // ← return the plain, usable token to client
        }

        public async Task<(string? NewAccessToken, string? NewRefreshToken, string? Error)> RefreshAsync(string rawRefreshToken, string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(rawRefreshToken))
                return (null, null, "Refresh token was not provided.");

            var hashedToken = _jwtTokenService.HashRefreshToken(rawRefreshToken);

            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.TokenHash == hashedToken);

            if (storedToken == null)
                return (null, null, "Refresh token is invalid (not found).");

            if (storedToken.IsRevoked)
                return (null, null, "This refresh token has already been revoked.");

            if (storedToken.ExpiresAt < DateTime.UtcNow)
                return (null, null, "Refresh token has expired.");

            var user = await _context.Users.FindAsync(storedToken.UserId);
            if (user == null)
                return (null, null, "The user associated with this token was not found.");

            // Get roles
            var roles = await _context.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name!)
                .ToListAsync();

            var newAccessToken = _jwtTokenService.GenerateToken(user, roles);

            // Rotation
            storedToken.IsRevoked = true;
            storedToken.RevokedByIp = ipAddress;
            storedToken.ReplacedByTokenHash = hashedToken;

            var newRawRefresh = _jwtTokenService.GenerateRefreshToken();
            var newHashed = _jwtTokenService.HashRefreshToken(newRawRefresh);

            var newDbToken = new RefreshToken
            {
                UserId = user.Id,
                TokenHash = newHashed,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                RevokedByIp = ipAddress
            };

            _context.RefreshTokens.Add(newDbToken);
            await _context.SaveChangesAsync();

            // Simple logging for debugging (you can use ILogger in production)
            Console.WriteLine($"Refresh successful. Old token revoked. New refresh issued for user {user.Id}");

            return (newAccessToken, newRawRefresh, null);
        }

        public async Task RevokeAllForUserAsync(string userId)
        {
            var tokens = await _context.RefreshTokens
                .Where(t => t.UserId == userId && !t.IsRevoked)
                .ToListAsync();

            foreach (var t in tokens)
            {
                t.IsRevoked = true;
            }

            await _context.SaveChangesAsync();
        }
    }
}
