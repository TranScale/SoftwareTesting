using System;

namespace OnlineSalesManagementSystem.Services.Security;

public static class PermissionConstants
{
    // Policy format: "PERM:Module.Action"
    public const string PolicyPrefix = "PERM:";
    public const string Wildcard = "*";

    // Stored permission format: "Module.Action"
    public static string Permission(string module, string action) => $"{module}.{action}";
    public static string PolicyName(string module, string action) => $"{PolicyPrefix}{Permission(module, action)}";

    public static bool TryParsePermissionFromPolicy(string policyName, out string permission)
    {
        permission = string.Empty;

        if (string.IsNullOrWhiteSpace(policyName)) return false;
        if (!policyName.StartsWith(PolicyPrefix, StringComparison.OrdinalIgnoreCase)) return false;

        permission = policyName.Substring(PolicyPrefix.Length).Trim();
        return !string.IsNullOrWhiteSpace(permission);
    }

    // Supports both:
    //  - "Module.Action" (new)
    //  - "Permissions.Module.Action" (legacy)
    public static bool TryParsePermission(string permission, out string module, out string action)
    {
        module = string.Empty;
        action = string.Empty;

        if (string.IsNullOrWhiteSpace(permission)) return false;

        var parts = permission.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length == 2)
        {
            module = parts[0];
            action = parts[1];
            return true;
        }

        if (parts.Length == 3 && parts[0].Equals("Permissions", StringComparison.OrdinalIgnoreCase))
        {
            module = parts[1];
            action = parts[2];
            return true;
        }

        return false;
    }

    public static class Modules
    {
        public const string Dashboard = "Dashboard";
        public const string Admin = "Admin";
        public const string AdminGroups = "AdminGroups";

        public const string Suppliers = "Suppliers";
        public const string Customers = "Customers";
        public const string Employees = "Employees";
        public const string Attendance = "Attendance";

        public const string Categories = "Categories";
        public const string Units = "Units";
        public const string Brands = "Brands";
        public const string Products = "Products";

        public const string Purchases = "Purchases";
        public const string Invoices = "Invoices";
        public const string Expenses = "Expenses";

        public const string Reports = "Reports";
        public const string Stock = "Stock";
        public const string Settings = "Settings";

        public static readonly string[] All = new[]
        {
            Dashboard, Admin, AdminGroups,
            Suppliers, Customers, Employees, Attendance,
            Categories, Units, Brands, Products,
            Purchases, Invoices, Expenses,
            Reports, Stock, Settings
        };
    }

    public static string[] AllModules => Modules.All;

    public static class Actions
    {
        public const string Show = "Show";
        // Legacy alias: n?u ch? no cn dng View th v?n map v? Show
        public const string View = Show;

        public const string Create = "Create";
        public const string Edit = "Edit";
        public const string Delete = "Delete";

        public const string Approve = "Approve";
        public const string Export = "Export";
        public static readonly string[] All = new[]
        {
            Show, Create, Edit, Delete, Approve, Export
        };
    }

    public static string[] AllActions => Actions.All;

    // Keep method name from old file so code cu khng gy
    public static string Build(string module, string action) => Permission(module, action);
    public static class Policies
    {
        public static string Require(string module, string action)
            => $"{PolicyPrefix}{module}.{action}";
    };
}
 