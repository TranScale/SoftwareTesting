// FILE: OnlineSalesManagementSystem/Areas/Admin/Controllers/SettingsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSalesManagementSystem.Services.Security;
using OnlineSalesManagementSystem.Data;
using OnlineSalesManagementSystem.Domain.Entities;

namespace OnlineSalesManagementSystem.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Settings + "." + PermissionConstants.Actions.Show)]
public class SettingsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<SettingsController> _logger;
    private const long MaxLogoUploadBytes = 2 * 1024 * 1024;
    private static readonly HashSet<string> AllowedLogoExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".webp"
    };

    public SettingsController(ApplicationDbContext db, IWebHostEnvironment env, ILogger<SettingsController> logger)
    {
        _db = db;
        _env = env;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var setting = await _db.Settings.FirstOrDefaultAsync();
        if (setting == null)
        {
            setting = new Setting
            {
                CompanyName = "Online Sales Management System",
                Currency = "VND",
                LogoPath = null
            };
            _db.Settings.Add(setting);
            await _db.SaveChangesAsync();
        }

        return View(setting);
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Settings + "." + PermissionConstants.Actions.Edit)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(Setting model, IFormFile? logoFile)
    {
        model.CompanyName = (model.CompanyName ?? string.Empty).Trim();
        model.Currency = string.IsNullOrWhiteSpace(model.Currency) ? "VND" : model.Currency.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(model.CompanyName))
        {
            ModelState.AddModelError(nameof(model.CompanyName), "Company name is required.");
        }

        TryValidateLogoUpload(logoFile);

        if (!ModelState.IsValid)
            return View(model);

        var setting = await _db.Settings.FirstOrDefaultAsync();
        if (setting == null)
        {
            setting = new Setting();
            _db.Settings.Add(setting);
        }

        model.LogoPath = setting.LogoPath;
        setting.CompanyName = model.CompanyName;
        setting.Currency = model.Currency;

        if (logoFile != null && logoFile.Length > 0)
        {
            var ext = Path.GetExtension(logoFile.FileName).ToLowerInvariant();
            var uploads = Path.Combine(_env.WebRootPath, "uploads", "logos");
            Directory.CreateDirectory(uploads);

            var fileName = $"logo_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(uploads, fileName);

            await using (var fs = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                await logoFile.CopyToAsync(fs);
            }

            setting.LogoPath = $"/uploads/logos/{fileName}";
            model.LogoPath = setting.LogoPath;
        }

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save system settings.");
            ModelState.AddModelError(string.Empty, "Cannot save settings right now. Please try again.");
            return View(model);
        }

        TempData["ToastSuccess"] = "Settings saved.";
        return RedirectToAction(nameof(Index));
    }

    private void TryValidateLogoUpload(IFormFile? logoFile)
    {
        if (logoFile == null || logoFile.Length == 0)
        {
            return;
        }

        if (logoFile.Length > MaxLogoUploadBytes)
        {
            ModelState.AddModelError(string.Empty, "Logo is too large. Maximum size is 2 MB.");
        }

        var ext = Path.GetExtension(logoFile.FileName).ToLowerInvariant();
        if (!AllowedLogoExtensions.Contains(ext))
        {
            ModelState.AddModelError(string.Empty, "Logo must be .png/.jpg/.jpeg/.webp");
        }
    }
}
