using Microsoft.AspNetCore.Authorization;

namespace OnlineSalesManagementSystem.Services.Security;

public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(string permission)
    {
        Permission = permission ?? throw new ArgumentNullException(nameof(permission));
    }

    public string Permission { get; }
}