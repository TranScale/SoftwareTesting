using Microsoft.EntityFrameworkCore;
using OnlineSalesManagementSystem.Domain.Entities;

namespace OnlineSalesManagementSystem.Data;

public static class DemoDataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        // Only seed once (delete DB if you want to re-seed)
        if (await db.Products.AnyAsync())
            return;

        // ===== Master data (structured similar to a women's gym/yoga store) =====
        var categories = new[]
        {
            new Category { Name = "Áo tập & Bra", Description = "Áo bra, croptop, áo thun thể thao (nữ)" },
            new Category { Name = "Quần/Legging", Description = "Legging, short, chân váy thể thao" },
            new Category { Name = "Set tập", Description = "Bộ đồng bộ (áo + quần) cho gym/yoga" },
            new Category { Name = "Áo khoác", Description = "Áo khoác mỏng, croptop jacket" },
            new Category { Name = "Phụ kiện", Description = "Băng đô, găng tay, phụ kiện tập luyện" }
        };
        db.Categories.AddRange(categories);

        var units = new[]
        {
            new Unit { Name = "Cái", ShortName = "cái", IsActive = true },
            new Unit { Name = "Bộ", ShortName = "bộ", IsActive = true },
            new Unit { Name = "Đôi", ShortName = "đôi", IsActive = true }
        };
        db.Units.AddRange(units);

        var suppliers = new[]
        {
            new Supplier { Name = "FitWear VN (Demo)", Phone = "0900000001", Email = "supplier1@demo.local", Address = "TP.HCM" },
            new Supplier { Name = "ActiveTextile Co. (Demo)", Phone = "0900000002", Email = "supplier2@demo.local", Address = "Hà Nội" }
        };
        db.Suppliers.AddRange(suppliers);

        var customers = new[]
        {
            new Customer { Name = "Khách lẻ", Phone = "", Email = "", Address = "" },
            new Customer { Name = "Nguyễn An", Phone = "0901234567", Email = "a@demo.local", Address = "Quận 1" },
            new Customer { Name = "Trần Bình", Phone = "0902345678", Email = "b@demo.local", Address = "Quận 7" }
        };
        db.Customers.AddRange(customers);

        var employees = new[]
        {
            new Employee { Name = "Lê Thanh", Position = "Sales", Salary = 12000000 },
            new Employee { Name = "Mai Hương", Position = "Warehouse", Salary = 10000000 }
        };
        db.Employees.AddRange(employees);

        await db.SaveChangesAsync();

        // Handy lookups
        var catTop = categories.First(c => c.Name == "Áo tập & Bra");
        var catBottom = categories.First(c => c.Name == "Quần/Legging");
        var catSet = categories.First(c => c.Name == "Set tập");
        var catOuter = categories.First(c => c.Name == "Áo khoác");
        var catAcc = categories.First(c => c.Name == "Phụ kiện");

        var unitPiece = units.First(u => u.Name == "Cái");
        var unitSet = units.First(u => u.Name == "Bộ");
        var unitPair = units.First(u => u.Name == "Đôi");

        // Deterministic placeholder images (no UI changes needed)
        static string Img(string sku) => $"https://dummyimage.com/600x600/eeeeee/333333&text={Uri.EscapeDataString(sku)}";

        var products = new[]
        {
            // Tops/Bra
            new Product { SKU="BRA-STRAP-01", Name="Áo bra dây chéo nâng đỡ", CategoryId=catTop.Id, UnitId=unitPiece.Id, CostPrice=115000, SalePrice=279000, StockOnHand=25, ReorderLevel=6, ImagePath=Img("BRA-STRAP-01"), IsActive=true },
            new Product { SKU="BRA-SCULPT-02", Name="Áo bra định hình lưng chữ Y", CategoryId=catTop.Id, UnitId=unitPiece.Id, CostPrice=125000, SalePrice=299000, StockOnHand=22, ReorderLevel=6, ImagePath=Img("BRA-SCULPT-02"), IsActive=true },
            new Product { SKU="TOP-CROP-03", Name="Áo croptop co giãn 4 chiều", CategoryId=catTop.Id, UnitId=unitPiece.Id, CostPrice=105000, SalePrice=239000, StockOnHand=30, ReorderLevel=8, ImagePath=Img("TOP-CROP-03"), IsActive=true },
            new Product { SKU="TEE-OVER-04", Name="Áo thun oversize thoáng khí", CategoryId=catTop.Id, UnitId=unitPiece.Id, CostPrice=95000, SalePrice=199000, StockOnHand=35, ReorderLevel=10, ImagePath=Img("TEE-OVER-04"), IsActive=true },

            // Bottoms
            new Product { SKU="LEG-HIGH-05", Name="Quần legging cạp cao ôm dáng", CategoryId=catBottom.Id, UnitId=unitPiece.Id, CostPrice=145000, SalePrice=269000, StockOnHand=28, ReorderLevel=8, ImagePath=Img("LEG-HIGH-05"), IsActive=true },
            new Product { SKU="LEG-CROSS-06", Name="Quần legging cạp chéo tôn eo", CategoryId=catBottom.Id, UnitId=unitPiece.Id, CostPrice=155000, SalePrice=289000, StockOnHand=24, ReorderLevel=8, ImagePath=Img("LEG-CROSS-06"), IsActive=true },
            new Product { SKU="SRT-2LAYER-07", Name="Quần short 2 lớp tập luyện", CategoryId=catBottom.Id, UnitId=unitPiece.Id, CostPrice=125000, SalePrice=229000, StockOnHand=26, ReorderLevel=8, ImagePath=Img("SRT-2LAYER-07"), IsActive=true },
            new Product { SKU="SKT-SPORT-08", Name="Chân váy thể thao (tennis style)", CategoryId=catBottom.Id, UnitId=unitPiece.Id, CostPrice=135000, SalePrice=259000, StockOnHand=18, ReorderLevel=6, ImagePath=Img("SKT-SPORT-08"), IsActive=true },

            // Sets
            new Product { SKU="SET-BRALEG-09", Name="Set bra + legging đồng bộ", CategoryId=catSet.Id, UnitId=unitSet.Id, CostPrice=260000, SalePrice=499000, StockOnHand=12, ReorderLevel=4, ImagePath=Img("SET-BRALEG-09"), IsActive=true },
            new Product { SKU="SET-TOPSHORT-10", Name="Set croptop + short năng động", CategoryId=catSet.Id, UnitId=unitSet.Id, CostPrice=235000, SalePrice=459000, StockOnHand=10, ReorderLevel=4, ImagePath=Img("SET-TOPSHORT-10"), IsActive=true },

            // Outerwear
            new Product { SKU="JKT-CROP-11", Name="Áo khoác croptop mỏng nhẹ", CategoryId=catOuter.Id, UnitId=unitPiece.Id, CostPrice=175000, SalePrice=339000, StockOnHand=14, ReorderLevel=4, ImagePath=Img("JKT-CROP-11"), IsActive=true },

            // Accessories
            new Product { SKU="ACC-HEADBAND-12", Name="Băng đô thể thao thấm mồ hôi", CategoryId=catAcc.Id, UnitId=unitPiece.Id, CostPrice=15000, SalePrice=39000, StockOnHand=60, ReorderLevel=20, ImagePath=Img("ACC-HEADBAND-12"), IsActive=true },
            new Product { SKU="ACC-GLOVES-13", Name="Găng tay tập gym (1 đôi)", CategoryId=catAcc.Id, UnitId=unitPair.Id, CostPrice=45000, SalePrice=89000, StockOnHand=40, ReorderLevel=15, ImagePath=Img("ACC-GLOVES-13"), IsActive=true }
        };
        db.Products.AddRange(products);

        await db.SaveChangesAsync();

        // ===== One received purchase (stock in) =====
        var sup1 = suppliers[0];
        var purchase = new Purchase
        {
            PurchaseNo = $"PUR-{Guid.NewGuid().ToString("N")[..10].ToUpperInvariant()}",
            SupplierId = sup1.Id,
            PurchaseDate = DateTime.UtcNow.Date.AddDays(-2),
            Status = PurchaseStatus.Received
        };

        purchase.Items.Add(new PurchaseItem
        {
            ProductId = products[0].Id,
            Qty = 12,
            UnitCost = products[0].CostPrice,
            LineTotal = 12 * products[0].CostPrice
        });
        purchase.Items.Add(new PurchaseItem
        {
            ProductId = products[4].Id,
            Qty = 18,
            UnitCost = products[4].CostPrice,
            LineTotal = 18 * products[4].CostPrice
        });

        purchase.SubTotal = purchase.Items.Sum(i => i.LineTotal);
        purchase.GrandTotal = purchase.SubTotal;

        db.Purchases.Add(purchase);
        await db.SaveChangesAsync();

        foreach (var it in purchase.Items)
        {
            var prod = await db.Products.FirstAsync(p => p.Id == it.ProductId);
            prod.StockOnHand += it.Qty;

            db.StockMovements.Add(new StockMovement
            {
                ProductId = it.ProductId,
                MovementDate = DateTime.UtcNow.Date.AddDays(-2),
                Type = StockMovementType.In,
                Qty = it.Qty,
                RefType = "Purchase",
                RefId = purchase.Id,
                Note = purchase.PurchaseNo
            });
        }
        await db.SaveChangesAsync();

        // ===== One invoice (stock out) =====
        var inv = new Invoice
        {
            InvoiceNo = $"INV-{Guid.NewGuid().ToString("N")[..10].ToUpperInvariant()}",
            CustomerId = customers[1].Id,
            InvoiceDate = DateTime.UtcNow.Date.AddDays(-1),
            Status = InvoiceStatus.Paid
        };

        inv.Items.Add(new InvoiceItem
        {
            ProductId = products[1].Id,
            Quantity = 2,
            UnitPrice = products[1].SalePrice,
            LineTotal = 2 * products[1].SalePrice
        });
        inv.Items.Add(new InvoiceItem
        {
            ProductId = products[6].Id,
            Quantity = 1,
            UnitPrice = products[6].SalePrice,
            LineTotal = 1 * products[6].SalePrice
        });

        inv.SubTotal = inv.Items.Sum(i => i.LineTotal);
        inv.GrandTotal = inv.SubTotal;
        inv.PaidAmount = inv.GrandTotal;

        db.Invoices.Add(inv);
        await db.SaveChangesAsync();

        foreach (var it in inv.Items)
        {
            var prod = await db.Products.FirstAsync(p => p.Id == it.ProductId);
            prod.StockOnHand -= it.Quantity;

            db.StockMovements.Add(new StockMovement
            {
                ProductId = it.ProductId,
                MovementDate = DateTime.UtcNow.Date.AddDays(-1),
                Type = StockMovementType.Out,
                Qty = it.Quantity,
                RefType = "Invoice",
                RefId = inv.Id,
                Note = inv.InvoiceNo
            });
        }
        await db.SaveChangesAsync();

        // ===== One expense =====
        db.Expenses.Add(new Expense
        {
            Title = "Chi phí vận hành (demo)",
            Amount = 850000,
            ExpenseDate = DateTime.UtcNow.Date.AddDays(-1),
            Note = "Dữ liệu demo"
        });

        await db.SaveChangesAsync();
    }
}
 