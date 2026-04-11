#nullable enable
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using OnlineSalesManagementSystem.Services.Security;

namespace OnlineSalesManagementSystem.Security;

/// <summary>
/// Optional: adds the current user's permissions as claims at request time.
/// This file is safe even if you don't register IClaimsTransformation.
/// </summary>
public sealed class PermissionClaimsTransformation : IClaimsTransformation
{
    public const string ClaimType = "permission";

    private readonly IPermissionService _permissionService;

    public PermissionClaimsTransformation(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        // Never return null to avoid CS8603
        if (principal?.Identity?.IsAuthenticated != true)
            return principal ?? new ClaimsPrincipal();

        var permissions = await _permissionService.GetUserPermissionsAsync(principal);

        if (permissions.Count == 0)
            return principal;

        // Add a separate identity so we don't mutate the original identity
        var identity = new ClaimsIdentity();
        var added = false;

        foreach (var perm in permissions)
        {
            if (!principal.HasClaim(ClaimType, perm))
            {
                identity.AddClaim(new Claim(ClaimType, perm));
                added = true;
            }
        }

        if (added)
        {
            principal.AddIdentity(identity);
        }

        return principal;
    }
}
