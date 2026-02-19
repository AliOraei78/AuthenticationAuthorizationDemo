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

        public async Task<(string? AccessToken, string? NewRefreshToken, string? Error)>
            RefreshAsync(string rawRefreshToken, string ipAddress)
        {
            var hashedToken = _jwtTokenService.HashRefreshToken(rawRefreshToken);

            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.TokenHash == hashedToken && !t.IsRevoked);

            if (storedToken == null || storedToken.ExpiresAt < DateTime.UtcNow)
                return (null, null, "Refresh token is invalid or expired.");

            var user = await _context.Users.FindAsync(storedToken.UserId);
            if (user == null)
                return (null, null, "User not found.");

            var roles = await _context.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                .ToListAsync();

            var newAccessToken = _jwtTokenService.GenerateToken(user, roles);

            // Rotation: revoke the old token and create a new one
            storedToken.IsRevoked = true;
            storedToken.RevokedByIp = ipAddress;
            storedToken.ReplacedByTokenHash =
                _jwtTokenService.HashRefreshToken(rawRefreshToken); // chain

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
