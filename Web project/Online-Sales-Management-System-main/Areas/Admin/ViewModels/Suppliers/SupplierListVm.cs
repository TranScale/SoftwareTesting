using System.ComponentModel.DataAnnotations;
using OnlineSalesManagementSystem.Areas.Admin.ViewModels.Common;

namespace OnlineSalesManagementSystem.Areas.Admin.ViewModels.Suppliers;

public class SupplierListVm
{
    public string? Query { get; set; }

    public PagedResult<SupplierRowVm> Data { get; set; } = new PagedResult<SupplierRowVm>(Array.Empty<SupplierRowVm>(), 0, 1, 10);

    // For UI hiding (Create/Edit/Delete buttons)
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}

public class SupplierRowVm
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class SupplierEditVm
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
}
