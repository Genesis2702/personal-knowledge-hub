using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Policy.Requirements;

namespace PersonalKnowledgeHub.Policy.Handlers;

public class OwnerOrAdminHandler : AuthorizationHandler<OwnerOrAdminRequirement, Resource>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OwnerOrAdminRequirement requirement,
        Resource resource)
    {
        var claim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(claim, out int userId))
        {
            return Task.CompletedTask;
        }
        if (context.User.IsInRole("Admin") || resource.UserId == userId)
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}