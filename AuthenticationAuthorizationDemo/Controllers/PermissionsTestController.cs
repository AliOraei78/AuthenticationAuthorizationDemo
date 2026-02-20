using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class PermissionsTestController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public PermissionsTestController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpGet("view-users")]
    [Authorize(Policy = "CanViewUsers")]
    public IActionResult ViewUsers()
    {
        return Ok("The list of users is viewable.");
    }

    [HttpGet("manage-users")]
    [Authorize(Policy = "CanManageUsers")]
    public IActionResult ManageUsers()
    {
        return Ok("User management is allowed.");
    }

    // Temporary diagnostic endpoint (add to AccountController or new controller)
    [HttpGet("debug-role-claims")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DebugRoleClaims()
    {
        var user = await _userManager.GetUserAsync(User);
        var roles = await _userManager.GetRolesAsync(user);

        var result = new List<object>();

        foreach (var roleName in roles)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                var claims = await _roleManager.GetClaimsAsync(role);
                result.Add(new
                {
                    Role = roleName,
                    PermissionClaims = claims.Where(c => c.Type == "Permission").Select(c => c.Value).ToList()
                });
            }
        }

        return Ok(result);
    }
}