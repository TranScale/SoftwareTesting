using System.ComponentModel.DataAnnotations;

namespace OnlineSalesManagementSystem.Domain.Entities;

public class GroupPermission
{
    public int Id { get; set; }

    [Required]
    public int AdminGroupId { get; set; }
    public AdminGroup? AdminGroup { get; set; }

    [Required, MaxLength(80)]
    public string Module { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Action { get; set; } = string.Empty;
}