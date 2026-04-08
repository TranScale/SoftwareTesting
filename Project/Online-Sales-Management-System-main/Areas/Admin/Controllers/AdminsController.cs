using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSalesManagementSystem.Data;
using OnlineSalesManagementSystem.Domain.Entities;
using OnlineSalesManagementSystem.Services.Security;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace OnlineSalesManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AdminsController> _logger;

        public AdminsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, ILogger<AdminsController> logger)
        {
            _db = db;
            _userManager = userManager;
            _logger = logger;
        }

        [PermissionAuthorize(PermissionConstants.Modules.Admin, PermissionConstants.Actions.Show)]
        public async Task<IActionResult> Index(string? q)
        {
            IQueryable<ApplicationUser> query = _db.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var s = q.Trim();
                query = query.Where(u =>
                    (u.Email != null && u.Email.Contains(s)) ||
                    (u.UserName != null && u.UserName.Contains(s)) ||
                    (u.FullName != null && u.FullName.Contains(s)));
            }

            var users = await query
                .OrderBy(u => u.Email)
                .ToListAsync();

            ViewBag.Query = q;
            return View(users);
        }

        [PermissionAuthorize(PermissionConstants.Modules.Admin, PermissionConstants.Actions.Create)]
        public async Task<IActionResult> Create()
        {
            ViewBag.Groups = await _db.AdminGroups.AsNoTracking().Where(g => g.IsActive).OrderBy(g => g.Name).ToListAsync();
            return View(new AdminCreateVm { IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize(PermissionConstants.Modules.Admin, PermissionConstants.Actions.Create)]
        public async Task<IActionResult> Create(AdminCreateVm vm)
        {
            ViewBag.Groups = await _db.AdminGroups.AsNoTracking().Where(g => g.IsActive).OrderBy(g => g.Name).ToListAsync();

            vm.Email = (vm.Email ?? string.Empty).Trim();
            vm.FullName = string.IsNullOrWhiteSpace(vm.FullName) ? null : vm.FullName.Trim();

            if (vm.AdminGroupId.HasValue &&
                !await _db.AdminGroups.AsNoTracking().AnyAsync(g => g.Id == vm.AdminGroupId.Value && g.IsActive))
            {
                ModelState.AddModelError(nameof(vm.AdminGroupId), "Selected group is invalid.");
            }

            if (!ModelState.IsValid) return View(vm);

            if (SuperAdminProtection.IsSuperAdminEmail(vm.Email))
            {
                ModelState.AddModelError(nameof(vm.Email), "This email is reserved for the Super Admin account.");
                return View(vm);
            }

            var user = new ApplicationUser
            {
                UserName = vm.Email?.Trim(),
                Email = vm.Email?.Trim(),
                FullName = vm.FullName?.Trim(),
                IsActive = vm.IsActive,
                AdminGroupId = vm.AdminGroupId
            };

            var result = await _userManager.CreateAsync(user, vm.Password ?? string.Empty);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
                return View(vm);
            }

            TempData["ToastSuccess"] = "Admin account created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [PermissionAuthorize(PermissionConstants.Modules.Admin, PermissionConstants.Actions.Edit)]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();

            ViewBag.Groups = await _db.AdminGroups.AsNoTracking().Where(g => g.IsActive).OrderBy(g => g.Name).ToListAsync();

            var vm = new AdminEditVm
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                AdminGroupId = user.AdminGroupId,
                IsActive = user.IsActive
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize(PermissionConstants.Modules.Admin, PermissionConstants.Actions.Edit)]
        
        public async Task<IActionResult> Edit(AdminEditVm vm)
        {
            ViewBag.Groups = await _db.AdminGroups.AsNoTracking().Where(g => g.IsActive).OrderBy(g => g.Name).ToListAsync();

            vm.Email = (vm.Email ?? string.Empty).Trim();
            vm.FullName = string.IsNullOrWhiteSpace(vm.FullName) ? null : vm.FullName.Trim();

            if (vm.AdminGroupId.HasValue &&
                !await _db.AdminGroups.AsNoTracking().AnyAsync(g => g.Id == vm.AdminGroupId.Value && g.IsActive))
            {
                ModelState.AddModelError(nameof(vm.AdminGroupId), "Selected group is invalid.");
            }

            if (!ModelState.IsValid) return View(vm);

            try
            {
                var user = await _userManager.FindByIdAsync(vm.Id);
                if (user == null) return NotFound();

                // Only Super Admin can edit the Super Admin account (UI already locks; server must enforce too)
                var meEmail = User?.FindFirstValue(ClaimTypes.Email);
                bool currentIsSuper = SuperAdminProtection.IsSuperAdminEmail(meEmail);
                bool targetIsSuper = SuperAdminProtection.IsSuperAdminEmail(user.Email);

                if (targetIsSuper && !currentIsSuper)
                {
                    TempData["ToastError"] = "You cannot modify the Super Admin account.";
                    return RedirectToAction(nameof(Index));
                }

                user.FullName = vm.FullName?.Trim();
                user.AdminGroupId = vm.AdminGroupId;

                // IsActive can be changed only for non-super admin accounts
                if (!targetIsSuper)
                    user.IsActive = vm.IsActive;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    foreach (var e in result.Errors)
                        ModelState.AddModelError(string.Empty, e.Description);

                    TempData["ToastError"] = "Failed to save changes.";
                    return View(vm);
                }

                // Optional password reset (leave blank to keep current password)
                if (!string.IsNullOrWhiteSpace(vm.NewPassword))
                {
                    var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, vm.NewPassword);

                    if (!resetResult.Succeeded)
                    {
                        foreach (var e in resetResult.Errors)
                            ModelState.AddModelError(string.Empty, e.Description);

                        TempData["ToastError"] = "Failed to update password.";
                        return View(vm);
                    }
                }

                TempData["ToastSuccess"] = "Admin account updated.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating admin account {AdminId}.", vm.Id);
                TempData["ToastError"] = "Unexpected error while saving changes.";
                ModelState.AddModelError(string.Empty, "Save failed. Please try again.");
                return View(vm);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize(PermissionConstants.Modules.Admin, PermissionConstants.Actions.Edit)]
        public async Task<IActionResult> Activate(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var myId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(myId) && string.Equals(myId, user.Id, StringComparison.OrdinalIgnoreCase))
            {
                TempData["ToastError"] = "You cannot change your own active status.";
                return RedirectToAction(nameof(Index));
            }

            // Block super admin
            if (SuperAdminProtection.IsSuperAdminEmail(user.Email))
            {
                TempData["ToastError"] = "You cannot activate/deactivate the Super Admin account.";
                return RedirectToAction(nameof(Index));
            }

            user.IsActive = true;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                TempData["ToastError"] = "Account activation failed.";
                return RedirectToAction(nameof(Index));
            }

            TempData["ToastSuccess"] = "Account activated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize(PermissionConstants.Modules.Admin, PermissionConstants.Actions.Delete)]
        public async Task<IActionResult> Disable(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var myId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(myId) && string.Equals(myId, user.Id, StringComparison.OrdinalIgnoreCase))
            {
                TempData["ToastError"] = "You cannot deactivate your own account.";
                return RedirectToAction(nameof(Index));
            }

            // Block super admin
            if (SuperAdminProtection.IsSuperAdminEmail(user.Email))
            {
                TempData["ToastError"] = "You cannot activate/deactivate the Super Admin account.";
                return RedirectToAction(nameof(Index));
            }

            user.IsActive = false;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                TempData["ToastError"] = "Account deactivation failed.";
                return RedirectToAction(nameof(Index));
            }

            TempData["ToastSuccess"] = "Account deactivated.";
            return RedirectToAction(nameof(Index));
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize(PermissionConstants.Modules.Admin, PermissionConstants.Actions.Delete)]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Block super admin
            if (SuperAdminProtection.IsSuperAdminEmail(user.Email))
            {
                TempData["ToastError"] = "You cannot delete the Super Admin account.";
                return RedirectToAction(nameof(Index));
            }

            // Prevent deleting yourself
            var myId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(myId) && string.Equals(myId, user.Id, StringComparison.OrdinalIgnoreCase))
            {
                TempData["ToastError"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Index));
            }

            // Soft delete: chỉ ẩn (deactivate) chứ KHÔNG xoá khỏi DB
            user.IsActive = false;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                TempData["ToastError"] = "Delete failed.";
            }
            else
            {
                TempData["ToastSuccess"] = "Admin account deleted (disabled).";
            }

            return RedirectToAction(nameof(Index));
        }

public class AdminCreateVm
        {
            [Required, EmailAddress, MaxLength(150)]
            public string Email { get; set; } = string.Empty;

            [MaxLength(150)]
            public string? FullName { get; set; }

            [DataType(DataType.Password)]
            [MinLength(6)]
            public string? NewPassword { get; set; }


            [Required]
            public string? Password { get; set; }

            public int? AdminGroupId { get; set; }

            public bool IsActive { get; set; } = true;
        }

        public class AdminEditVm
        {
            [Required]
            public string Id { get; set; } = string.Empty;

            [Required, EmailAddress, MaxLength(150)]
            public string Email { get; set; } = string.Empty;

            [MaxLength(150)]
            public string? FullName { get; set; }


            [DataType(DataType.Password)]
            [MinLength(6)]
            public string? NewPassword { get; set; }
            public int? AdminGroupId { get; set; }

            public bool IsActive { get; set; } = true;
        }
    }
}
 
 
