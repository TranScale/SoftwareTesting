using System.ComponentModel.DataAnnotations;

namespace OnlineSalesManagementSystem.Areas.Admin.ViewModels.AdminGroups;

public class AdminGroupIndexVm
{
    public AdminGroupEditVm Create { get; set; } = new();
    public List<AdminGroupRowVm> Groups { get; set; } = new();
}

public class AdminGroupRowVm
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool HasWildcard { get; set; }
}

public class AdminGroupEditVm
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }
}

public class AdminGroupPermissionsVm
{
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;

    public List<string> Modules { get; set; } = new();
    public List<string> Actions { get; set; } = new();

    // Checkbox binding target
    public List<string> SelectedPermissions { get; set; } = new();
}
