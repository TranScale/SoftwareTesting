using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineSalesManagementSystem.Data;
using OnlineSalesManagementSystem.Domain.Entities;
using OnlineSalesManagementSystem.Services.Security;
using AppUser = OnlineSalesManagementSystem.Domain.Entities.ApplicationUser;

namespace OnlineSalesManagementSystem.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var config = services.GetRequiredService<IConfiguration>();
        var initMode = config.GetValue<string>("Database:InitializationMode") ?? "EnsureCreated";

        if (initMode.Equals("Migrate", StringComparison.OrdinalIgnoreCase))
        {
            await db.Database.MigrateAsync();
        }
        else if (initMode.Equals("EnsureCreated", StringComparison.OrdinalIgnoreCase))
        {
            await db.Database.EnsureCreatedAsync();
        }
        else
        {
            throw new InvalidOperationException(
                $"Unsupported Database:InitializationMode '{initMode}'. Use 'EnsureCreated' or 'Migrate'.");
        }

        // 1) Roles
        const string adminRole = "Admin";
        if (!await roleManager.RoleExistsAsync(adminRole))
        {
            await roleManager.CreateAsync(new IdentityRole(adminRole));
        }

        // 2) Admin groups
        var super = await db.AdminGroups.FirstOrDefaultAsync(g => g.Name == "Super Admin");
        if (super == null)
        {
            super = new AdminGroup { Name = "Super Admin", Description = "Full access to all modules.", IsActive = true };
            db.AdminGroups.Add(super);
            await db.SaveChangesAsync();
        }
        else if (!super.IsActive)
        {
            super.IsActive = true;
            await db.SaveChangesAsync();
        }

        var sales = await db.AdminGroups.FirstOrDefaultAsync(g => g.Name == "Sales Staff");
        if (sales == null)
        {
            sales = new AdminGroup { Name = "Sales Staff", Description = "Sales user with limited permissions.", IsActive = true };
            db.AdminGroups.Add(sales);
            await db.SaveChangesAsync();
        }
        else if (!sales.IsActive)
        {
            sales.IsActive = true;
            await db.SaveChangesAsync();
        }

        // 3) Group permissions
        await EnsureGroupPermissionsAsync(db, super.Id, grantAll: true);
        await EnsureGroupPermissionsAsync(db, sales.Id, grantAll: false);

        // 4) Default users
        await EnsureUserAsync(userManager, adminRole,
            email: "admin@osms.local",
            password: "Admin@12345",
            fullName: "Super Admin",
            adminGroupId: super.Id);

        await EnsureUserAsync(userManager, adminRole,
            email: "sales@osms.local",
            password: "Sales@12345",
            fullName: "Sales Staff",
            adminGroupId: sales.Id);

        // 5) Demo domain data (fixes old seeding that referenced Product.SupplierId)
        await DemoDataSeeder.SeedAsync(db);
    }

    private static async Task EnsureUserAsync(
        UserManager<AppUser> userManager,
        string role,
        string email,
        string password,
        string fullName,
        int adminGroupId)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new AppUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                IsActive = true,
                AdminGroupId = adminGroupId,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Cannot create user '{email}': {errors}");
            }
        }
        else
        {
            // keep it alive + correct group
            user.IsActive = true;
            user.AdminGroupId = adminGroupId;
            user.FullName = string.IsNullOrWhiteSpace(user.FullName) ? fullName : user.FullName;
            await userManager.UpdateAsync(user);
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }

    private static async Task EnsureGroupPermissionsAsync(ApplicationDbContext db, int groupId, bool grantAll)
    {
        // Wipe existing permissions for deterministic seed
        var existing = await db.GroupPermissions.Where(p => p.AdminGroupId == groupId).ToListAsync();
        if (existing.Count > 0)
        {
            db.GroupPermissions.RemoveRange(existing);
            await db.SaveChangesAsync();
        }

        var toAdd = new List<GroupPermission>();

        if (grantAll)
        {
            foreach (var module in PermissionConstants.Modules.All)
            foreach (var action in PermissionConstants.Actions.All)
                toAdd.Add(new GroupPermission { AdminGroupId = groupId, Module = module, Action = action });
        }
        else
        {
            // "Video-like" sales staff permissions: can see and create sales records, but cannot delete critical data
            string[] modules =
            {
                PermissionConstants.Modules.Dashboard,
                PermissionConstants.Modules.Customers,
                PermissionConstants.Modules.Products,
                PermissionConstants.Modules.Invoices,
                PermissionConstants.Modules.Purchases,
                PermissionConstants.Modules.Stock,
                PermissionConstants.Modules.Reports,
            };

            foreach (var module in modules)
            {
                toAdd.Add(new GroupPermission { AdminGroupId = groupId, Module = module, Action = PermissionConstants.Actions.Show });
            }

            foreach (var module in new[] { PermissionConstants.Modules.Invoices, PermissionConstants.Modules.Customers })
            {
                toAdd.Add(new GroupPermission { AdminGroupId = groupId, Module = module, Action = PermissionConstants.Actions.Create });
                toAdd.Add(new GroupPermission { AdminGroupId = groupId, Module = module, Action = PermissionConstants.Actions.Edit });
            }

            // Purchases: allow create drafts and receive (Edit)
            toAdd.Add(new GroupPermission { AdminGroupId = groupId, Module = PermissionConstants.Modules.Purchases, Action = PermissionConstants.Actions.Create });
            toAdd.Add(new GroupPermission { AdminGroupId = groupId, Module = PermissionConstants.Modules.Purchases, Action = PermissionConstants.Actions.Edit });

            // Products: allow edit stock-related fields but no delete
            toAdd.Add(new GroupPermission { AdminGroupId = groupId, Module = PermissionConstants.Modules.Products, Action = PermissionConstants.Actions.Edit });
        }

        db.GroupPermissions.AddRange(toAdd);
        await db.SaveChangesAsync();
    }
}
 
