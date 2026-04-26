using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Policy.Requirements;

namespace PersonalKnowledgeHub.Policy.Handlers;

public class TagOwnerOrAdminHandler : AuthorizationHandler<TagOwnerOrAdminRequirement, Tag>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        TagOwnerOrAdminRequirement requirement, Tag tag)
    {
        var claim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(claim, out int userId))
        {
            return Task.CompletedTask;
        }
        if (context.User.IsInRole("Admin") || tag.UserId == userId)
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}