using AuthenticationAuthorizationDemo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AuthenticationAuthorizationDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
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

            // Find user first to provide better error messages
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return BadRequest(ModelState);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user,
                model.Password,
                isPersistent: model.RememberMe,   // persistent = long-lived cookie
                lockoutOnFailure: true);           // enable lockout after failures

            if (result.Succeeded)
            {
                // Optional: Refresh security stamp if needed (rarely here)
                return Ok(new { Message = "Login successful." });
            }

            if (result.IsLockedOut)
            {
                return BadRequest(new
                {
                    Message = "Account locked out due to too many failed attempts. Try again later."
                });
            }

            if (result.RequiresTwoFactor)
            {
                return BadRequest(new { Message = "Two-factor authentication required." });
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return BadRequest(ModelState);
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
    }
}