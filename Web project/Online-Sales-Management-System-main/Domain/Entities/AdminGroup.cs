using System.ComponentModel.DataAnnotations;

namespace OnlineSalesManagementSystem.Domain.Entities;

public class AdminGroup
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }


    public bool IsActive { get; set; } = true;
    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    public ICollection<GroupPermission> Permissions { get; set; } = new List<GroupPermission>();
}
 