using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using FixedApp.Models;

namespace FixedApp.Authorization
{
    public class IsOwnerHandler : AuthorizationHandler<IsOwnerRequirement, Document>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IsOwnerRequirement requirement, Document resource)
        {
            if (!context.User.Identity.IsAuthenticated)
            {
                return Task.CompletedTask;
            }
            if (context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
            var uid = context.User.FindFirst(ClaimTypes.NameIdentifier);
            if (uid != null)
            {
                int currId = int.Parse(uid.Value);
                if (currId == resource.OwnerId)
                {
                    context.Succeed(requirement);
                }
            }
            return Task.CompletedTask;
        }
    }
}
