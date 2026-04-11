using System.ComponentModel.DataAnnotations;

namespace OnlineSalesManagementSystem.Domain.Entities;

public class Supplier
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(150), EmailAddress]
    public string? Email { get; set; }

    [MaxLength(300)]
    public string? Address { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}