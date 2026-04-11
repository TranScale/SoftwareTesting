using Microsoft.EntityFrameworkCore;
using OnlineSalesManagementSystem.Data;
using OnlineSalesManagementSystem.Domain.Entities;

namespace OnlineSalesManagementSystem.Services.Security;

/// <summary>
/// Central place to define and enforce "Super Admin" invariants.
/// UI should hide buttons, but server-side checks must still block the operations.
/// </summary>
public static class SuperAdminProtection
{
    public const string SuperAdminGroupName = "Super Admin";
    public const string SuperAdminEmail = "admin@osms.local";

    public static bool IsSuperAdminEmail(string? email)
        => !string.IsNullOrWhiteSpace(email)
           && string.Equals(email.Trim(), SuperAdminEmail, StringComparison.OrdinalIgnoreCase);

    public static bool IsSuperAdminGroupName(string? name)
        => !string.IsNullOrWhiteSpace(name)
           && string.Equals(name.Trim(), SuperAdminGroupName, StringComparison.OrdinalIgnoreCase);

    public static async Task<int?> GetSuperAdminGroupIdAsync(ApplicationDbContext db, CancellationToken ct = default)
    {
        return await db.AdminGroups
            .AsNoTracking()
            .Where(g => g.Name == SuperAdminGroupName)
            .Select(g => (int?)g.Id)
            .FirstOrDefaultAsync(ct);
    }

    public static async Task<bool> IsSuperAdminGroupIdAsync(ApplicationDbContext db, int? groupId, CancellationToken ct = default)
    {
        if (groupId == null) return false;

        return await db.AdminGroups
            .AsNoTracking()
            .AnyAsync(g => g.Id == groupId.Value && g.Name == SuperAdminGroupName, ct);
    }

    public static async Task<bool> IsSuperAdminUserAsync(ApplicationDbContext db, ApplicationUser? user, CancellationToken ct = default)
    {
        if (user == null) return false;

        if (IsSuperAdminEmail(user.Email))
            return true;

        return await IsSuperAdminGroupIdAsync(db, user.AdminGroupId, ct);
    }
}
