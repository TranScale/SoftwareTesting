using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ClosedXML.Excel;
using OnlineSalesManagementSystem.Data;
using OnlineSalesManagementSystem.Domain.Entities;
using OnlineSalesManagementSystem.Services.Security;

namespace OnlineSalesManagementSystem.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Products + "." + PermissionConstants.Actions.Show)]
public class ProductsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ProductsController> _logger;

    private const string ExcelImportCachePrefix = "ProductsExcelImport:";
    private const long MaxImageUploadBytes = 2 * 1024 * 1024;
    private const long MaxExcelUploadBytes = 10 * 1024 * 1024;
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".webp"
    };

    public ProductsController(ApplicationDbContext db, IWebHostEnvironment env, IMemoryCache cache, ILogger<ProductsController> logger)
    {
        _db = db;
        _env = env;
        _cache = cache;
        _logger = logger;
    }

    // ==========================================================
    // EXCEL IMPORT/EXPORT
    // ==========================================================

    [HttpGet]
    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Products + "." + PermissionConstants.Actions.Show)]
    public IActionResult ImportExcel()
    {
        // Upload page
        return View();
    }

    // ==========================================================
    // PRODUCT CRUD
    // ==========================================================

    [HttpGet]
    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Products + "." + PermissionConstants.Actions.Create)]
    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = await _db.Categories.AsNoTracking().Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();
        ViewBag.Units = await _db.Units.AsNoTracking().Where(u => u.IsActive).OrderBy(u => u.Name).ToListAsync();
        ViewBag.Brands = await _db.Brands.AsNoTracking().Where(b => b.IsActive).OrderBy(b => b.Name).ToListAsync();

        return View(new Product { IsActive = true });
    }

    [HttpPost]
    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Products + "." + PermissionConstants.Actions.Create)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product model, IFormFile? imageFile)
    {
        // Normalize
        model.SKU = (model.SKU ?? string.Empty).Trim();
        model.Name = (model.Name ?? string.Empty).Trim();
        model.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
        model.ImagePath = string.IsNullOrWhiteSpace(model.ImagePath) ? null : model.ImagePath.Trim();

        // Dropdown data (needed when return View)
        ViewBag.Categories = await _db.Categories.AsNoTracking().Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();
        ViewBag.Units = await _db.Units.AsNoTracking().Where(u => u.IsActive).OrderBy(u => u.Name).ToListAsync();
        ViewBag.Brands = await _db.Brands.AsNoTracking().Where(b => b.IsActive).OrderBy(b => b.Name).ToListAsync();

        // Fix "0" sentinel from UI
        if (model.CategoryId.HasValue && model.CategoryId.Value == 0) model.CategoryId = null;
        if (model.UnitId.HasValue && model.UnitId.Value == 0) model.UnitId = null;
        if (model.BrandId.HasValue && model.BrandId.Value == 0) model.BrandId = null;

        await ValidateProductForWriteAsync(model, isCreate: true);
        TryValidateImageUpload(imageFile);

        if (!ModelState.IsValid)
            return View(model);

        try
        {
            // Handle upload image (optional)
            if (imageFile != null && imageFile.Length > 0)
            {
                var ext = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

                var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", "products");
                Directory.CreateDirectory(uploadsRoot);

                var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{ext}";
                var abs = Path.Combine(uploadsRoot, fileName);
                await using (var fs = new FileStream(abs, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    await imageFile.CopyToAsync(fs);
                }

                model.ImagePath = $"/uploads/products/{fileName}";
            }

            model.IsActive = true;
            _db.Products.Add(model);
            await _db.SaveChangesAsync();

            TempData["ToastSuccess"] = "Product created.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create product with SKU {Sku}.", model.SKU);
            ModelState.AddModelError(string.Empty, "Cannot create product right now. Please try again.");
            return View(model);
        }
    }

    [HttpPost]
    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Products + "." + PermissionConstants.Actions.Create)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportExcelPreview(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["ToastError"] = "Vui lòng chọn file Excel.";
            return RedirectToAction(nameof(ImportExcel));
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".xlsx")
        {
            TempData["ToastError"] = "Chỉ hỗ trợ file .xlsx";
            return RedirectToAction(nameof(ImportExcel));
        }

        if (file.Length > MaxExcelUploadBytes)
        {
            TempData["ToastError"] = "File quá lớn (tối đa 10MB).";
            return RedirectToAction(nameof(ImportExcel));
        }

        try
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            ms.Position = 0;

            using var wb = new XLWorkbook(ms);
            var ws = wb.Worksheets.FirstOrDefault();
            if (ws == null)
            {
                TempData["ToastError"] = "File Excel không có worksheet.";
                return RedirectToAction(nameof(ImportExcel));
            }

            var rows = await ParseProductsFromWorksheetAsync(ws);

            var cacheKey = Guid.NewGuid().ToString("N");
            _cache.Set(ExcelImportCachePrefix + cacheKey, rows, TimeSpan.FromMinutes(20));

            var vm = new ProductExcelPreviewVm
            {
                CacheKey = cacheKey,
                Rows = rows,
                TotalRows = rows.Count,
                ValidRows = rows.Count(r => r.IsValid),
                InvalidRows = rows.Count(r => !r.IsValid)
            };

            return View("ImportExcelPreview", vm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to preview product Excel import for file {FileName}.", file.FileName);
            TempData["ToastError"] = "Không đọc được file Excel. Vui lòng kiểm tra lại file.";
            return RedirectToAction(nameof(ImportExcel));
        }
    }

    [HttpPost]
    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Products + "." + PermissionConstants.Actions.Create)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportExcelConfirm(string cacheKey)
    {
        if (string.IsNullOrWhiteSpace(cacheKey))
        {
            TempData["ToastError"] = "Thiếu dữ liệu preview.";
            return RedirectToAction(nameof(ImportExcel));
        }

        if (!_cache.TryGetValue(ExcelImportCachePrefix + cacheKey, out List<ProductExcelRowVm>? rows) || rows == null)
        {
            TempData["ToastError"] = "Dữ liệu preview đã hết hạn. Vui lòng upload lại.";
            return RedirectToAction(nameof(ImportExcel));
        }

        var valid = rows.Where(r => r.IsValid).ToList();
        if (valid.Count == 0)
        {
            TempData["ToastError"] = "Không có dòng hợp lệ để import.";
            return RedirectToAction(nameof(ImportExcel));
        }

        using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            foreach (var r in valid)
            {
                // Upsert theo SKU
                var sku = (r.SKU ?? "").Trim();
                if (string.IsNullOrWhiteSpace(sku)) continue;

                var existing = await _db.Products.FirstOrDefaultAsync(p => p.SKU.ToLower() == sku.ToLower());
                if (existing == null)
                {
                    existing = new Product { SKU = sku };
                    _db.Products.Add(existing);
                }

                existing.Name = r.Name!.Trim();
                existing.CategoryId = r.ResolvedCategoryId;
                existing.UnitId = r.ResolvedUnitId;
                existing.BrandId = r.ResolvedBrandId;
                existing.CostPrice = r.CostPrice ?? 0;
                existing.SalePrice = r.SalePrice ?? 0;
                existing.StockOnHand = r.StockOnHand ?? existing.StockOnHand;
                existing.ReorderLevel = r.ReorderLevel ?? existing.ReorderLevel;
                existing.IsActive = r.IsActive ?? true;
                existing.IsTrending = r.IsTrending ?? false;
                existing.ImagePath = string.IsNullOrWhiteSpace(r.ImagePath) ? existing.ImagePath : r.ImagePath;
                existing.Description = r.Description;
                existing.Content = r.Content;
            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            _cache.Remove(ExcelImportCachePrefix + cacheKey);
            TempData["ToastSuccess"] = $"Import Excel thành công: {valid.Count} dòng.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            _logger.LogError(ex, "Failed to import product Excel with cache key {CacheKey}.", cacheKey);
            TempData["ToastError"] = "Import thất bại. Vui lòng thử lại.";
            return RedirectToAction(nameof(ImportExcel));
        }
    }

    [HttpGet]
    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Products + "." + PermissionConstants.Actions.Show)]
    public async Task<IActionResult> ExportExcel()
    {
        var products = await _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Unit)
            .Include(p => p.Brand)
            .OrderBy(p => p.Id)
            .ToListAsync();

        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Products");

        // Header
        var headers = new[]
        {
            "SKU*","Name*","CategoryId","CategoryName","UnitId","UnitName","BrandId","BrandName",
            "CostPrice","SalePrice","StockOnHand","ReorderLevel","IsActive","IsTrending","ImagePath","Description","Content"
        };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
            ws.Cell(1, i + 1).Style.Font.Bold = true;
        }

        int row = 2;
        foreach (var p in products)
        {
            ws.Cell(row, 1).Value = p.SKU;
            ws.Cell(row, 2).Value = p.Name;
            ws.Cell(row, 3).Value = p.CategoryId;
            ws.Cell(row, 4).Value = p.Category?.Name;
            ws.Cell(row, 5).Value = p.UnitId;
            ws.Cell(row, 6).Value = p.Unit?.Name;
            ws.Cell(row, 7).Value = p.BrandId;
            ws.Cell(row, 8).Value = p.Brand?.Name;
            ws.Cell(row, 9).Value = p.CostPrice;
            ws.Cell(row, 10).Value = p.SalePrice;
            ws.Cell(row, 11).Value = p.StockOnHand;
            ws.Cell(row, 12).Value = p.ReorderLevel;
            ws.Cell(row, 13).Value = p.IsActive;
            ws.Cell(row, 14).Value = p.IsTrending;
            ws.Cell(row, 15).Value = p.ImagePath;
            ws.Cell(row, 16).Value = p.Description;
            ws.Cell(row, 17).Value = p.Content;
            row++;
        }

        ws.Columns().AdjustToContents();

        using var outMs = new MemoryStream();
        wb.SaveAs(outMs);
        outMs.Position = 0;

        return File(outMs.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"products_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }

    [HttpGet]
    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Products + "." + PermissionConstants.Actions.Show)]
    public IActionResult ExportExcelTemplate()
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Template");

        var headers = new[]
        {
            "SKU*","Name*","CategoryId","CategoryName","UnitId","UnitName","BrandId","BrandName",
            "CostPrice","SalePrice","StockOnHand","ReorderLevel","IsActive","IsTrending","ImagePath","Description","Content"
        };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
            ws.Cell(1, i + 1).Style.Font.Bold = true;
        }
        ws.Cell(2, 1).Value = "SP001";
        ws.Cell(2, 2).Value = "Ví dụ sản phẩm";
        ws.Cell(2, 4).Value = "Đồ gia dụng";
        ws.Cell(2, 6).Value = "Chiếc";
        ws.Cell(2, 9).Value = 100000;
        ws.Cell(2, 10).Value = 120000;
        ws.Cell(2, 11).Value = 10;
        ws.Cell(2, 13).Value = true;
        ws.Cell(2, 14).Value = false;
        ws.Cell(2, 16).Value = "Mô tả ngắn";
        ws.Cell(2, 17).Value = "<p>Nội dung chi tiết</p>";

        ws.Columns().AdjustToContents();

        using var outMs = new MemoryStream();
        wb.SaveAs(outMs);
        outMs.Position = 0;

        return File(outMs.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "products_template.xlsx");
    }

    private async Task<List<ProductExcelRowVm>> ParseProductsFromWorksheetAsync(IXLWorksheet ws)
    {
        // Build lookup dictionaries
        var categories = await _db.Categories.AsNoTracking().ToListAsync();
        var units = await _db.Units.AsNoTracking().ToListAsync();
        var brands = await _db.Brands.AsNoTracking().ToListAsync();

        var categoryById = categories.ToDictionary(c => c.Id, c => c);
        var unitById = units.ToDictionary(u => u.Id, u => u);
        var brandById = brands.ToDictionary(b => b.Id, b => b);

        var categoryByName = categories
            .Where(c => !string.IsNullOrWhiteSpace(c.Name))
            .GroupBy(c => c.Name!.Trim().ToLower())
            .ToDictionary(g => g.Key, g => g.First());

        var unitByName = units
            .Where(u => !string.IsNullOrWhiteSpace(u.Name))
            .GroupBy(u => u.Name!.Trim().ToLower())
            .ToDictionary(g => g.Key, g => g.First());

        var brandByName = brands
            .Where(b => !string.IsNullOrWhiteSpace(b.Name))
            .GroupBy(b => b.Name!.Trim().ToLower())
            .ToDictionary(g => g.Key, g => g.First());

        // Read header row and map columns
        var headerRow = ws.Row(1);
        var lastCol = headerRow.LastCellUsed()?.Address.ColumnNumber ?? 0;
        var colMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int c = 1; c <= lastCol; c++)
        {
            var key = headerRow.Cell(c).GetString().Trim();
            if (!string.IsNullOrWhiteSpace(key) && !colMap.ContainsKey(key))
                colMap[key] = c;
        }

        int GetCol(params string[] names)
        {
            foreach (var n in names)
                if (colMap.TryGetValue(n, out var idx)) return idx;
            return -1;
        }

        var skuCol = GetCol("SKU", "SKU*");
        var nameCol = GetCol("Name", "Name*");
        var categoryIdCol = GetCol("CategoryId");
        var categoryNameCol = GetCol("CategoryName");
        var unitIdCol = GetCol("UnitId");
        var unitNameCol = GetCol("UnitName");
        var brandIdCol = GetCol("BrandId");
        var brandNameCol = GetCol("BrandName");
        var costCol = GetCol("CostPrice");
        var saleCol = GetCol("SalePrice");
        var stockCol = GetCol("StockOnHand");
        var reorderCol = GetCol("ReorderLevel");
        var activeCol = GetCol("IsActive");
        var trendingCol = GetCol("IsTrending");
        var imageCol = GetCol("ImagePath", "ImageUrl");
        var descCol = GetCol("Description");
        var contentCol = GetCol("Content");

        if (skuCol < 0 || nameCol < 0)
            throw new InvalidOperationException("File thiếu cột bắt buộc: SKU và Name.");

        var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
        var results = new List<ProductExcelRowVm>();

        for (int r = 2; r <= lastRow; r++)
        {
            var row = ws.Row(r);
            if (row.IsEmpty()) continue;

            var vm = new ProductExcelRowVm
            {
                RowNo = r,
                SKU = row.Cell(skuCol).GetString().Trim(),
                Name = row.Cell(nameCol).GetString().Trim(),
                CategoryId = ReadInt(row, categoryIdCol),
                CategoryName = ReadString(row, categoryNameCol),
                UnitId = ReadInt(row, unitIdCol),
                UnitName = ReadString(row, unitNameCol),
                BrandId = ReadInt(row, brandIdCol),
                BrandName = ReadString(row, brandNameCol),
                CostPrice = ReadDecimal(row, costCol),
                SalePrice = ReadDecimal(row, saleCol),
                StockOnHand = ReadInt(row, stockCol),
                ReorderLevel = ReadInt(row, reorderCol),
                IsActive = ReadBool(row, activeCol),
                IsTrending = ReadBool(row, trendingCol),
                ImagePath = ReadString(row, imageCol),
                Description = ReadString(row, descCol),
                Content = ReadString(row, contentCol)
            };

            // Validate
            if (string.IsNullOrWhiteSpace(vm.SKU)) vm.Errors.Add("SKU bắt buộc.");
            if (string.IsNullOrWhiteSpace(vm.Name)) vm.Errors.Add("Name bắt buộc.");

            // Resolve category
            if (vm.CategoryId.HasValue)
            {
                if (categoryById.TryGetValue(vm.CategoryId.Value, out var c)) vm.ResolvedCategoryId = c.Id;
                else vm.Errors.Add($"CategoryId không tồn tại: {vm.CategoryId}");
            }
            else if (!string.IsNullOrWhiteSpace(vm.CategoryName))
            {
                var k = vm.CategoryName.Trim().ToLower();
                if (categoryByName.TryGetValue(k, out var c)) vm.ResolvedCategoryId = c.Id;
                else vm.Errors.Add($"CategoryName không tồn tại: {vm.CategoryName}");
            }

            // Resolve unit
            if (vm.UnitId.HasValue)
            {
                if (unitById.TryGetValue(vm.UnitId.Value, out var u)) vm.ResolvedUnitId = u.Id;
                else vm.Errors.Add($"UnitId không tồn tại: {vm.UnitId}");
            }
            else if (!string.IsNullOrWhiteSpace(vm.UnitName))
            {
                var k = vm.UnitName.Trim().ToLower();
                if (unitByName.TryGetValue(k, out var u)) vm.ResolvedUnitId = u.Id;
                else vm.Errors.Add($"UnitName không tồn tại: {vm.UnitName}");
            }

            // Resolve brand (optional)
            if (vm.BrandId.HasValue)
            {
                if (brandById.TryGetValue(vm.BrandId.Value, out var b)) vm.ResolvedBrandId = b.Id;
                else vm.Errors.Add($"BrandId không tồn tại: {vm.BrandId}");
            }
            else if (!string.IsNullOrWhiteSpace(vm.BrandName))
            {
                var k = vm.BrandName.Trim().ToLower();
                if (brandByName.TryGetValue(k, out var b)) vm.ResolvedBrandId = b.Id;
                else vm.Errors.Add($"BrandName không tồn tại: {vm.BrandName}");
            }

            if (vm.CostPrice.HasValue && vm.CostPrice.Value < 0) vm.Errors.Add("CostPrice không được âm.");
            if (vm.SalePrice.HasValue && vm.SalePrice.Value < 0) vm.Errors.Add("SalePrice không được âm.");
            if (vm.StockOnHand.HasValue && vm.StockOnHand.Value < 0) vm.Errors.Add("StockOnHand không được âm.");
            if (vm.ReorderLevel.HasValue && vm.ReorderLevel.Value < 0) vm.Errors.Add("ReorderLevel không được âm.");

            vm.IsValid = vm.Errors.Count == 0;
            results.Add(vm);
        }

        // Detect duplicate SKU inside file
        var dupSkus = results
            .Where(r => !string.IsNullOrWhiteSpace(r.SKU))
            .GroupBy(r => r.SKU!.Trim().ToLower())
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToHashSet();

        foreach (var r in results)
        {
            if (!string.IsNullOrWhiteSpace(r.SKU) && dupSkus.Contains(r.SKU.Trim().ToLower()))
            {
                r.Errors.Add("SKU bị trùng trong file Excel.");
                r.IsValid = false;
            }
        }

        return results;

        static string? ReadString(IXLRow row, int col)
        {
            if (col < 0) return null;
            var s = row.Cell(col).GetString();
            return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
        }

        static int? ReadInt(IXLRow row, int col)
        {
            if (col < 0) return null;
            var cell = row.Cell(col);
            if (cell.IsEmpty()) return null;
            if (cell.TryGetValue<int>(out var i)) return i;
            var s = cell.GetString();
            return int.TryParse(s, out var v) ? v : null;
        }

        static decimal? ReadDecimal(IXLRow row, int col)
        {
            if (col < 0) return null;
            var cell = row.Cell(col);
            if (cell.IsEmpty()) return null;
            if (cell.TryGetValue<decimal>(out var d)) return d;
            var s = cell.GetString();
            return decimal.TryParse(s, out var v) ? v : null;
        }

        static bool? ReadBool(IXLRow row, int col)
        {
            if (col < 0) return null;
            var cell = row.Cell(col);
            if (cell.IsEmpty()) return null;
            if (cell.TryGetValue<bool>(out var b)) return b;
            var s = cell.GetString().Trim().ToLower();
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (s is "1" or "true" or "yes" or "y" or "có" or "co") return true;
            if (s is "0" or "false" or "no" or "n" or "không" or "khong") return false;
            return null;
        }
    }

    public sealed class ProductExcelPreviewVm
    {
        public string CacheKey { get; set; } = "";
        public List<ProductExcelRowVm> Rows { get; set; } = new();
        public int TotalRows { get; set; }
        public int ValidRows { get; set; }
        public int InvalidRows { get; set; }
    }

    public sealed class ProductExcelRowVm
    {
        public int RowNo { get; set; }
        public string? SKU { get; set; }
        public string? Name { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int? UnitId { get; set; }
        public string? UnitName { get; set; }
        public int? BrandId { get; set; }
        public string? BrandName { get; set; }
        public decimal? CostPrice { get; set; }
        public decimal? SalePrice { get; set; }
        public int? StockOnHand { get; set; }
        public int? ReorderLevel { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsTrending { get; set; }
        public string? ImagePath { get; set; }
        public string? Description { get; set; }
        public string? Content { get; set; }

        // Resolved foreign keys
        public int? ResolvedCategoryId { get; set; }
        public int? ResolvedUnitId { get; set; }
        public int? ResolvedBrandId { get; set; }

        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 10)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var query = _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Unit)
            .Include(p => p.Brand)
            .Where(p => p.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(p => p.SKU.Contains(q) || p.Name.Contains(q));
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(p => p.IsTrending)
            .ThenBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Query = q;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.Total = total;

        return View(items);
    }

    // ====== DETAILS: thông tin sản phẩm + lịch sử hóa đơn có sản phẩm này ======
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var product = await _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Unit)
            .Include(p => p.Brand)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) return NotFound();

        var orders = await _db.Invoices
            .AsNoTracking()
            .Include(i => i.Customer)
            .Include(i => i.Items)
            .Where(i => i.Items.Any(it => it.ProductId == id))
            .OrderByDescending(i => i.InvoiceDate)
            .Select(i => new ProductOrderRow
            {
                InvoiceId = i.Id,
                InvoiceNo = i.InvoiceNo,
                CustomerId = i.CustomerId,
                CustomerName = i.Customer != null ? i.Customer.Name : "Khách lẻ",
                InvoiceDate = i.InvoiceDate,
                Qty = i.Items.Where(it => it.ProductId == id).Sum(it => it.Quantity),
                UnitPrice = i.Items.Where(it => it.ProductId == id).Select(it => it.UnitPrice).FirstOrDefault(),
                LineTotal = i.Items.Where(it => it.ProductId == id).Sum(it => it.LineTotal),
                Status = i.Status
            })
            .ToListAsync();

        var vm = new ProductDetailsVm
        {
            Product = product,
            Orders = orders,
            TotalSoldQty = orders.Sum(x => x.Qty),
            TotalRevenue = orders.Sum(x => x.LineTotal)
        };

        return View(vm);
    }

    // ==========================================================
    // CÁC HÀM EDIT ĐÃ ĐƯỢC BỔ SUNG ĐỂ SỬA LỖI 404
    // ==========================================================

    // 1. GET: Hiển thị form chỉnh sửa
    [HttpGet]
    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Products + "." + PermissionConstants.Actions.Edit)]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();

        // Lấy danh sách danh mục & đơn vị tính để hiển thị dropdown
        ViewBag.Categories = await _db.Categories.ToListAsync();
        ViewBag.Units = await _db.Units.ToListAsync();
        ViewBag.Brands = await _db.Brands.AsNoTracking().Where(b => b.IsActive).OrderBy(b => b.Name).ToListAsync();

        return View(product);
    }

    // ====== TOGGLE TRENDING ======
    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Products + "." + PermissionConstants.Actions.Edit)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleTrending(int id)
    {
        var entity = await _db.Products.FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        if (entity == null) return Json(new { success = false, message = "Not found" });

        entity.IsTrending = !entity.IsTrending;
        await _db.SaveChangesAsync();

        return Json(new { success = true, isTrending = entity.IsTrending });
    }

    // ====== VIEWMODELS ======
    public sealed class ProductDetailsVm
    {
        public Product Product { get; set; } = default!;
        public List<ProductOrderRow> Orders { get; set; } = new();
        public int TotalSoldQty { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public sealed class ProductOrderRow
    {
        public int InvoiceId { get; set; }
        public string InvoiceNo { get; set; } = "";
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; } = "";
        public DateTime InvoiceDate { get; set; }
        public int Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
        public InvoiceStatus Status { get; set; }
    }

    private async Task ValidateProductForWriteAsync(Product model, bool isCreate)
    {
        if (string.IsNullOrWhiteSpace(model.SKU)) ModelState.AddModelError(nameof(model.SKU), "SKU is required.");
        if (string.IsNullOrWhiteSpace(model.Name)) ModelState.AddModelError(nameof(model.Name), "Name is required.");
        if (model.CostPrice < 0) ModelState.AddModelError(nameof(model.CostPrice), "Cost price cannot be negative.");
        if (model.SalePrice < 0) ModelState.AddModelError(nameof(model.SalePrice), "Sale price cannot be negative.");
        if (model.ReorderLevel < 0) ModelState.AddModelError(nameof(model.ReorderLevel), "Reorder level cannot be negative.");

        if (model.CategoryId.HasValue &&
            !await _db.Categories.AsNoTracking().AnyAsync(c => c.Id == model.CategoryId.Value && c.IsActive))
        {
            ModelState.AddModelError(nameof(model.CategoryId), "Selected category is invalid.");
        }

        if (model.UnitId.HasValue &&
            !await _db.Units.AsNoTracking().AnyAsync(u => u.Id == model.UnitId.Value && u.IsActive))
        {
            ModelState.AddModelError(nameof(model.UnitId), "Selected unit is invalid.");
        }

        if (model.BrandId.HasValue &&
            !await _db.Brands.AsNoTracking().AnyAsync(b => b.Id == model.BrandId.Value && b.IsActive))
        {
            ModelState.AddModelError(nameof(model.BrandId), "Selected brand is invalid.");
        }

        var skuExists = await _db.Products.AsNoTracking().AnyAsync(p =>
            p.SKU == model.SKU &&
            p.IsActive &&
            (isCreate || p.Id != model.Id));

        if (skuExists)
        {
            ModelState.AddModelError(nameof(model.SKU), "SKU already exists.");
        }
    }

    private void TryValidateImageUpload(IFormFile? imageFile)
    {
        if (imageFile == null || imageFile.Length == 0)
        {
            return;
        }

        if (imageFile.Length > MaxImageUploadBytes)
        {
            ModelState.AddModelError(string.Empty, "Image is too large. Maximum size is 2 MB.");
        }

        var ext = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
        if (!AllowedImageExtensions.Contains(ext))
        {
            ModelState.AddModelError(string.Empty, "Image type not supported. Use png/jpg/jpeg/webp.");
        }
    }
}
