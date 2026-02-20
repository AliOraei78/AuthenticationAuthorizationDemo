using AuthenticationAuthorizationDemo.Models;
using AuthenticationAuthorizationDemo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AuthenticationAuthorizationDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly JwtTokenService _jwtTokenService;   // ← new
        private readonly RefreshTokenService _refreshTokenService;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            JwtTokenService jwtTokenService, 
            RefreshTokenService refreshTokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
            _refreshTokenService = refreshTokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                return Ok(new { Message = "User registered successfully. You can now log in." });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return BadRequest(ModelState);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return BadRequest(ModelState);
            }

            var result = await _signInManager.CheckPasswordSignInAsync(
                user,
                model.Password,
                lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                    return BadRequest(new { Message = "Account locked out due to too many failed attempts." });

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return BadRequest(ModelState);
            }

            // Login successful
            var roles = await _userManager.GetRolesAsync(user);

            // Generate access token
            var accessToken = await _jwtTokenService.GenerateToken(user);

            // Create and store refresh token → get the real saved token
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var refreshToken = await _refreshTokenService.CreateRefreshTokenAsync(user.Id, ip);

            return Ok(new
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresInMinutes = 15
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto request)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            var (newAccessToken, newRefreshToken, error) = await _refreshTokenService.RefreshAsync(request.RefreshToken, ip);

            if (error != null)
                return BadRequest(new { Message = error });

            return Ok(new
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }

        [HttpPost("logout")]
        [Authorize]  // Only logged-in users can log out
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return Ok(new { Message = "You have been successfully logged out." });
        }

        // Simple protected test endpoint
        [HttpGet("protected")]
        [Authorize]
        public IActionResult ProtectedResource()
        {
            return Ok(new
            {
                Message = "This is a protected resource – you are authenticated.",
                UserEmail = User.Identity?.Name
            });
        }

        [HttpPost("revoke")]
        [Authorize]  // Only authenticated users can revoke their own tokens
        public async Task<IActionResult> Revoke()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // or "sub" from JWT
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            await _refreshTokenService.RevokeAllForUserAsync(userId);

            return Ok(new { Message = "All your refresh tokens have been revoked." });
        }
    }
}