using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations; // Nếu cần

namespace OnlineSalesManagementSystem.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? AvatarPath { get; set; }
        public bool IsActive { get; set; }
        public int? AdminGroupId { get; set; }
        public AdminGroup? AdminGroup { get; set; }
    }
}