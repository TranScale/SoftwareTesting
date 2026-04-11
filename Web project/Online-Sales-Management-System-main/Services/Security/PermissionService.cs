using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using OnlineSalesManagementSystem.Services.Security;
using OnlineSalesManagementSystem.Data;

namespace OnlineSalesManagementSystem.Services.Security;

public interface IPermissionService
{
    Task<bool> UserHasPermissionAsync(ClaimsPrincipal user, string permission, CancellationToken ct = default);
    Task<HashSet<string>> GetUserPermissionsAsync(ClaimsPrincipal user, CancellationToken ct = default);
    Task<HashSet<string>> GetGroupPermissionsAsync(int adminGroupId, CancellationToken ct = default);
}

public sealed class PermissionService : IPermissionService
{
    private readonly ApplicationDbContext _db;

    public PermissionService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<bool> UserHasPermissionAsync(ClaimsPrincipal user, string permission, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(permission)) return false;

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return false;

        var appUser = await _db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new { u.IsActive, u.AdminGroupId })
            .FirstOrDefaultAsync(ct);

        if (appUser == null || !appUser.IsActive) return false;
        if (appUser.AdminGroupId == null) return false;

        var groupActive = await _db.AdminGroups.AsNoTracking().AnyAsync(g => g.Id == appUser.AdminGroupId.Value && g.IsActive, ct);
        if (!groupActive) return false;

        // Super simple evaluation: wildcard & exact
        // Supported:
        //   *.*  (stored as Module="*", Action="*")
        //   Module.* (Module="X", Action="*")
        //   *.Action (Module="*", Action="Show")
        //   Module.Action
        var parts = permission.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2) return false;

        var module = parts[0];
        var action = parts[1];

        return await _db.GroupPermissions
            .AsNoTracking()
            .AnyAsync(p =>
                p.AdminGroupId == appUser.AdminGroupId.Value &&
                (
                    (p.Module == PermissionConstants.Wildcard && p.Action == PermissionConstants.Wildcard) ||
                    (p.Module == module && p.Action == PermissionConstants.Wildcard) ||
                    (p.Module == PermissionConstants.Wildcard && p.Action == action) ||
                    (p.Module == module && p.Action == action)
                ), ct);
    }

    public async Task<HashSet<string>> GetUserPermissionsAsync(ClaimsPrincipal user, CancellationToken ct = default)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var groupId = await _db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId && u.IsActive)
            .Select(u => u.AdminGroupId)
            .FirstOrDefaultAsync(ct);

        if (groupId == null) return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var groupActive2 = await _db.AdminGroups.AsNoTracking().AnyAsync(g => g.Id == groupId.Value && g.IsActive, ct);
        if (!groupActive2) return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        return await GetGroupPermissionsAsync(groupId.Value, ct);
    }

    public async Task<HashSet<string>> GetGroupPermissionsAsync(int adminGroupId, CancellationToken ct = default)
    {
        var groupActive = await _db.AdminGroups.AsNoTracking().AnyAsync(g => g.Id == adminGroupId && g.IsActive, ct);
        if (!groupActive) return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var perms = await _db.GroupPermissions
            .AsNoTracking()
            .Where(p => p.AdminGroupId == adminGroupId)
            .Select(p => new { p.Module, p.Action })
            .ToListAsync(ct);

        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var p in perms)
        {
            set.Add($"{p.Module}.{p.Action}");
        }

        return set;
    }
}
 