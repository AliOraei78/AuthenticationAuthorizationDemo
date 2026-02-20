using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationAuthorizationDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PolicyTestController : ControllerBase
    {
        [HttpGet("adults-only")]
        [Authorize(Policy = "AtLeast18")]
        public IActionResult AdultsOnly()
        {
            return Ok(new
            {
                Message = "You are at least 18 years old and have access to this content.",
                User = User.Identity?.Name
            });
        }

        [HttpGet("admin-or-over21")]
        [Authorize(Policy = "AdminOrOver21")]
        public IActionResult AdminOrOver21()
        {
            return Ok(new
            {
                Message = "You are either an Admin or at least 21 years old.",
                User = User.Identity?.Name
            });
        }

        [HttpGet("email-confirmed")]
        [Authorize(Policy = "RequireClaimEmailConfirmed")]
        public IActionResult EmailConfirmedOnly()
        {
            return Ok(new
            {
                Message = "Your email has been confirmed.",
                User = User.Identity?.Name
            });
        }
    }
}