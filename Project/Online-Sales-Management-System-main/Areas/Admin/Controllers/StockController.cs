// FILE: OnlineSalesManagementSystem/Areas/Admin/Controllers/StockController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSalesManagementSystem.Services.Security;
using OnlineSalesManagementSystem.Data;
using OnlineSalesManagementSystem.Domain.Entities;
using ClosedXML.Excel;
using OnlineSalesManagementSystem.Services.Common;
using System.Globalization;
using System.IO;

namespace OnlineSalesManagementSystem.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Stock + "." + PermissionConstants.Actions.Show)]
public class StockController : Controller
{
    private readonly ApplicationDbContext _db;

    public StockController(ApplicationDbContext db)
    {
        _db = db;
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
            .Where(p => p.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(p => p.SKU.Contains(q) || p.Name.Contains(q));
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Query = q;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.Total = total;

        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Low(string? q)
    {
        var query = _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Unit)
            .Where(p => p.IsActive && p.StockOnHand <= p.ReorderLevel)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(p => p.SKU.Contains(q) || p.Name.Contains(q));
        }

        var items = await query.OrderBy(p => p.Name).ToListAsync();
        ViewBag.Query = q;

        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Movements(int? productId, DateTime? from, DateTime? to)
    {
        var f = from?.Date;
        var t = to?.Date;

        var query = _db.StockMovements
            .AsNoTracking()
            .Include(m => m.Product)
            .AsQueryable();

        if (productId.HasValue && productId.Value > 0)
            query = query.Where(m => m.ProductId == productId.Value);

        if (f.HasValue)
            query = query.Where(m => m.MovementDate >= f.Value);

        if (t.HasValue)
            query = query.Where(m => m.MovementDate <= t.Value.AddDays(1).AddTicks(-1));

        var items = await query
            .OrderByDescending(m => m.MovementDate)
            .ThenByDescending(m => m.Id)
            .Take(1000)
            .ToListAsync();

        ViewBag.ProductId = productId;
        ViewBag.From = f?.ToString("yyyy-MM-dd");
        ViewBag.To = t?.ToString("yyyy-MM-dd");

        ViewBag.Products = await _db.Products.AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();

        return View(items);
    }


    [HttpGet]
    public async Task<IActionResult> ExportMovementsExcel(int? productId, DateTime? from, DateTime? to)
    {
        var f = from?.Date;
        var tt = to?.Date;

        var query = _db.StockMovements
            .AsNoTracking()
            .Include(m => m.Product)
            .AsQueryable();

        if (productId.HasValue && productId.Value > 0)
            query = query.Where(m => m.ProductId == productId.Value);

        if (f.HasValue)
            query = query.Where(m => m.MovementDate >= f.Value);

        if (tt.HasValue)
            query = query.Where(m => m.MovementDate <= tt.Value.AddDays(1).AddTicks(-1));

        var items = await query
            .OrderByDescending(m => m.MovementDate)
            .ThenByDescending(m => m.Id)
            .ToListAsync();

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("XuatKho");

        // Header
        ws.Cell(1, 1).Value = "LICH SU XUAT/NHAP KHO";
        ws.Range("A1:G1").Merge().Style.Font.SetBold().Font.SetFontSize(14);
        ws.Cell(2, 1).Value = $"Tu: {(f.HasValue ? f.Value.ToString("dd/MM/yyyy") : "--")}  Den: {(tt.HasValue ? tt.Value.ToString("dd/MM/yyyy") : "--")}";
        ws.Range("A2:G2").Merge();

        int row = 4;
        ws.Cell(row, 1).Value = "Thoi gian";
        ws.Cell(row, 2).Value = "San pham";
        ws.Cell(row, 3).Value = "Loai";
        ws.Cell(row, 4).Value = "So luong";
        ws.Cell(row, 5).Value = "Tham chieu";
        ws.Cell(row, 6).Value = "Ma";
        ws.Cell(row, 7).Value = "Ghi chu";
        ws.Range(row, 1, row, 7).Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.LightGray);

        row++;

        foreach (var m in items)
        {
            var vnTime = AppTime.ToVietnamTime(m.MovementDate);
            ws.Cell(row, 1).Value = vnTime.ToString("HH:mm dd/MM/yyyy", CultureInfo.InvariantCulture);
            ws.Cell(row, 2).Value = m.Product != null ? $"{m.Product.SKU} - {m.Product.Name}" : $"#{m.ProductId}";
            ws.Cell(row, 3).Value = m.Type.ToString();
            ws.Cell(row, 4).Value = m.Qty;
            ws.Cell(row, 5).Value = m.RefType;
            ws.Cell(row, 6).Value = m.RefId;
            ws.Cell(row, 7).Value = m.Note;
            row++;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        stream.Position = 0;

        var fileName = $"stock_movements_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

}
