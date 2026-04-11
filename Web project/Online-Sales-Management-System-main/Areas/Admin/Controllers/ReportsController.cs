using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSalesManagementSystem.Services.Security;
using OnlineSalesManagementSystem.Data;
using OnlineSalesManagementSystem.Domain.Entities;
using ClosedXML.Excel;
using System.IO;

namespace OnlineSalesManagementSystem.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Reports + "." + PermissionConstants.Actions.Show)]
public class ReportsController : Controller
{
    private readonly ApplicationDbContext _db;

    public ReportsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(DateTime? from, DateTime? to)
    {
        var f = (from ?? DateTime.UtcNow.Date.AddDays(-30)).Date;
        var t = (to ?? DateTime.UtcNow.Date).Date;

        if (t < f)
        {
            var tmp = f; f = t; t = tmp;
        }

        var invoicesQuery = _db.Invoices
            .AsNoTracking()
            .Include(i => i.Customer)
            .Where(i => i.InvoiceDate >= f && i.InvoiceDate < t.AddDays(1) && i.Status != InvoiceStatus.Cancelled);

        var purchasesQuery = _db.Purchases
            .AsNoTracking()
            .Include(p => p.Supplier)
            .Where(p => p.PurchaseDate >= f && p.PurchaseDate < t.AddDays(1) && p.Status != PurchaseStatus.Cancelled);

        var invoices = await invoicesQuery
            .OrderByDescending(i => i.InvoiceDate)
            .ThenByDescending(i => i.Id)
            .Take(200)
            .ToListAsync();

        var purchases = await purchasesQuery
            .OrderByDescending(p => p.PurchaseDate)
            .ThenByDescending(p => p.Id)
            .Take(200)
            .ToListAsync();

        var salesTotal = await invoicesQuery.SumAsync(x => (decimal?)x.GrandTotal) ?? 0m;
        var purchaseTotal = await purchasesQuery.SumAsync(x => (decimal?)x.GrandTotal) ?? 0m;
        var profit = salesTotal - purchaseTotal;

        var vm = new ReportsIndexVm
        {
            From = f,
            To = t,
            SalesTotal = salesTotal,
            PurchaseTotal = purchaseTotal,
            Profit = profit,
            Invoices = invoices,
            Purchases = purchases
        };

        return View(vm);
    }

    // --- HÀM XUẤT EXCEL (ĐÃ CẬP NHẬT TÍNH TỔNG) ---
    [HttpGet]
    public async Task<IActionResult> ExportExcel(DateTime? from, DateTime? to)
    {
        var f = (from ?? DateTime.UtcNow.Date.AddDays(-30)).Date;
        var t = (to ?? DateTime.UtcNow.Date).Date;

        if (t < f) { var tmp = f; f = t; t = tmp; }

        // 1. Tạo Query (Lọc theo ngày, bỏ trạng thái Cancel)
        var invoicesQuery = _db.Invoices
            .AsNoTracking()
            .Include(i => i.Customer)
            .Where(i => i.InvoiceDate >= f && i.InvoiceDate < t.AddDays(1) && i.Status != InvoiceStatus.Cancelled);

        var purchasesQuery = _db.Purchases
            .AsNoTracking()
            .Include(p => p.Supplier)
            .Where(p => p.PurchaseDate >= f && p.PurchaseDate < t.AddDays(1) && p.Status != PurchaseStatus.Cancelled);

        // 2. Tính toán TỔNG (Phần bạn đang thiếu)
        var salesTotal = await invoicesQuery.SumAsync(x => (decimal?)x.GrandTotal) ?? 0m;
        var purchaseTotal = await purchasesQuery.SumAsync(x => (decimal?)x.GrandTotal) ?? 0m;
        var profit = salesTotal - purchaseTotal;

        // 3. Lấy dữ liệu chi tiết
        var invoices = await invoicesQuery
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();

        var purchases = await purchasesQuery
            .OrderByDescending(p => p.PurchaseDate)
            .ToListAsync();

        // 4. Tạo file Excel
        using (var workbook = new XLWorkbook())
        {
            // === SHEET 1: TỔNG QUAN (SUMMARY) ===
            var wsSummary = workbook.Worksheets.Add("Tong Quan");

            // Tiêu đề
            wsSummary.Cell(1, 1).Value = "BÁO CÁO TỔNG QUAN";
            wsSummary.Range("A1:C1").Merge().Style.Font.SetBold().Font.SetFontSize(14);

            wsSummary.Cell(2, 1).Value = $"Thời gian: {f:dd/MM/yyyy} - {t:dd/MM/yyyy}";
            wsSummary.Range("A2:C2").Merge();

            // Kẻ bảng số liệu
            wsSummary.Cell(4, 1).Value = "CHỈ SỐ";
            wsSummary.Cell(4, 2).Value = "GIÁ TRỊ (VNĐ)";
            var headerSum = wsSummary.Range("A4:B4");
            headerSum.Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.LightGray);

            // Ghi dữ liệu tổng
            wsSummary.Cell(5, 1).Value = "Tổng Doanh Thu";
            wsSummary.Cell(5, 2).Value = salesTotal;
            wsSummary.Cell(5, 2).Style.Font.SetFontColor(XLColor.DarkGreen).Font.SetBold();

            wsSummary.Cell(6, 1).Value = "Tổng Chi Phí";
            wsSummary.Cell(6, 2).Value = purchaseTotal;
            wsSummary.Cell(6, 2).Style.Font.SetFontColor(XLColor.DarkRed).Font.SetBold();

            wsSummary.Cell(7, 1).Value = "Lợi Nhuận Ròng";
            wsSummary.Cell(7, 2).Value = profit;
            var profitCell = wsSummary.Cell(7, 2);
            profitCell.Style.Font.SetBold();
            if (profit >= 0) profitCell.Style.Font.SetFontColor(XLColor.Blue);
            else profitCell.Style.Font.SetFontColor(XLColor.Red);

            // Format số tiền
            wsSummary.Range("B5:B7").Style.NumberFormat.Format = "#,##0";
            wsSummary.Columns().AdjustToContents();


            // === SHEET 2: CHI TIẾT DOANH THU ===
            var wsSales = workbook.Worksheets.Add("Chi Tiet Doanh Thu");
            wsSales.Cell(1, 1).Value = "Mã Hóa Đơn";
            wsSales.Cell(1, 2).Value = "Ngày Lập";
            wsSales.Cell(1, 3).Value = "Khách Hàng";
            wsSales.Cell(1, 4).Value = "Tổng Tiền";
            wsSales.Cell(1, 5).Value = "Trạng Thái";

            var headerSales = wsSales.Range("A1:E1");
            headerSales.Style.Font.Bold = true;
            headerSales.Style.Fill.BackgroundColor = XLColor.LightGreen;

            int row = 2;
            foreach (var item in invoices)
            {
                wsSales.Cell(row, 1).Value = item.InvoiceNo;
                wsSales.Cell(row, 2).Value = item.InvoiceDate;
                wsSales.Cell(row, 3).Value = item.Customer?.Name ?? "Khách lẻ";
                wsSales.Cell(row, 4).Value = item.GrandTotal;
                wsSales.Cell(row, 4).Style.NumberFormat.Format = "#,##0";
                wsSales.Cell(row, 5).Value = item.Status.ToString();
                row++;
            }
            wsSales.Columns().AdjustToContents();


            // === SHEET 3: CHI TIẾT CHI PHÍ ===
            var wsCost = workbook.Worksheets.Add("Chi Tiet Nhap Hang");
            wsCost.Cell(1, 1).Value = "Mã Đơn Nhập";
            wsCost.Cell(1, 2).Value = "Ngày Nhập";
            wsCost.Cell(1, 3).Value = "Nhà Cung Cấp";
            wsCost.Cell(1, 4).Value = "Tổng Tiền";
            wsCost.Cell(1, 5).Value = "Trạng Thái";

            var headerCost = wsCost.Range("A1:E1");
            headerCost.Style.Font.Bold = true;
            headerCost.Style.Fill.BackgroundColor = XLColor.LightCoral;

            row = 2;
            foreach (var item in purchases)
            {
                wsCost.Cell(row, 1).Value = item.PurchaseNo;
                wsCost.Cell(row, 2).Value = item.PurchaseDate;
                wsCost.Cell(row, 3).Value = item.Supplier?.Name ?? "-";
                wsCost.Cell(row, 4).Value = item.GrandTotal;
                wsCost.Cell(row, 4).Style.NumberFormat.Format = "#,##0";
                wsCost.Cell(row, 5).Value = item.Status.ToString();
                row++;
            }
            wsCost.Columns().AdjustToContents();

            // Xuất file
            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                var content = stream.ToArray();
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"BaoCao_{DateTime.Now:ddMMyyyy}.xlsx");
            }
        }
    }

    public sealed class ReportsIndexVm
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }

        public decimal SalesTotal { get; set; }
        public decimal PurchaseTotal { get; set; }
        public decimal Profit { get; set; }

        public List<Invoice> Invoices { get; set; } = new();
        public List<Purchase> Purchases { get; set; } = new();
    }
}