using System.ComponentModel.DataAnnotations;

namespace OnlineSalesManagementSystem.Areas.Admin.ViewModels.Auth;

public class LoginViewModel
{
    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}