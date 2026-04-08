using System.ComponentModel.DataAnnotations;

namespace OnlineSalesManagementSystem.Domain.Entities;

public class Expense
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal Amount { get; set; }

    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;

    public string? Note { get; set; }

    public bool IsActive { get; set; } = true;
}
 