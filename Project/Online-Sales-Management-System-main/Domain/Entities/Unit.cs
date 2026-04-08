using System.ComponentModel.DataAnnotations;

namespace OnlineSalesManagementSystem.Domain.Entities;

public class Unit
{
    public int Id { get; set; }

    [Required, MaxLength(80)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? ShortName { get; set; }

    public bool IsActive { get; set; } = true;
}
 