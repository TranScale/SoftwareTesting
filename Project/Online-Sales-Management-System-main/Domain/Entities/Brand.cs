using System.ComponentModel.DataAnnotations;

namespace OnlineSalesManagementSystem.Domain.Entities;

public class Brand
{
    public int Id { get; set; }
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}