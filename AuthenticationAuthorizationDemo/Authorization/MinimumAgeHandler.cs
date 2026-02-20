using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AuthenticationAuthorizationDemo.Authorization
{
    public class MinimumAgeHandler : AuthorizationHandler<MinimumAgeRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            MinimumAgeRequirement requirement)
        {
            // We assume the "DateOfBirth" claim exists in the token (we will add it later)
            var birthDateClaim = context.User.FindFirst("DateOfBirth")?.Value;

            if (string.IsNullOrEmpty(birthDateClaim) ||
                !DateTime.TryParse(birthDateClaim, out var birthDate))
            {
                return Task.CompletedTask; // Claim does not exist or is invalid → fail
            }

            var age = DateTime.Today.Year - birthDate.Year;
            if (birthDate.Date > DateTime.Today.AddYears(-age)) age--;

            if (age >= requirement.MinimumAge)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}