using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationAuthorizationDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesTestController : ControllerBase
    {
        // Only users with the Admin role can access this action
        [HttpGet("admin-only")]
        [Authorize(Roles = "Admin")]
        public IActionResult AdminOnly()
        {
            return Ok(new
            {
                Message = "This content is only visible to users with the Admin role.",
                User = User.Identity?.Name
            });
        }

        // Only users with the User role
        [HttpGet("user-only")]
        [Authorize(Roles = "User")]
        public IActionResult UserOnly()
        {
            return Ok(new
            {
                Message = "This content is only visible to users with the User role.",
                User = User.Identity?.Name
            });
        }

        // Users who have at least one of the Admin or User roles
        [HttpGet("admin-or-user")]
        [Authorize(Roles = "Admin,User")]
        public IActionResult AdminOrUser()
        {
            return Ok(new
            {
                Message = "This content is visible to Admin or User roles.",
                User = User.Identity?.Name
            });
        }

        // Only authenticated users (no role restriction)
        [HttpGet("authenticated-only")]
        [Authorize]
        public IActionResult AuthenticatedOnly()
        {
            return Ok(new
            {
                Message = "This content is visible to any authenticated user.",
                User = User.Identity?.Name
            });
        }
    }
}