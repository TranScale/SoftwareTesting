using Microsoft.AspNetCore.Authorization;

namespace OnlineSalesManagementSystem.Services.Security;

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPermissionService _permissionService;

    public PermissionAuthorizationHandler(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
            return;

        var ok = await _permissionService.UserHasPermissionAsync(context.User, requirement.Permission);
        if (ok)
        {
            context.Succeed(requirement);
        }
    }
}
