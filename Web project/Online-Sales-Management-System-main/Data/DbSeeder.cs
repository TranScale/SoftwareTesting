using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineSalesManagementSystem.Domain.Entities;
using OnlineSalesManagementSystem.Services.Security;

namespace OnlineSalesManagementSystem.Data
{
    public static class DbSeeder
    {
        /// <summary>
        /// Seed dữ liệu mẫu (idempotent).
        /// - resetDemoData = true: xóa dữ liệu nghiệp vụ (products/invoices/purchases/...) rồi seed lại.
        ///   Không xóa Identity users để tránh mất tài khoản login.
        /// </summary>
        public static async Task SeedAsync(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            bool resetDemoData = false)
        {
            if (resetDemoData)
            {
                await ResetBusinessDataAsync(db);
            }

            await SeedMasterDataAsync(db);
            var (superGroup, warehouseGroup, salesGroup) = await SeedGroupsAndPermissionsAsync(db);
            await SeedUsersAsync(userManager, superGroup.Id, warehouseGroup.Id, salesGroup.Id);
            await SeedBusinessDataAsync(db);
        }

        private static async Task SeedMasterDataAsync(ApplicationDbContext db)
        {
            if (!await db.Units.AnyAsync())
            {
                db.Units.AddRange(
                    new Unit { Name = "Cái", ShortName = "cái", IsActive = true },
                    new Unit { Name = "Hộp", ShortName = "hộp", IsActive = true },
                    new Unit { Name = "Bộ", ShortName = "bộ", IsActive = true },
                    new Unit { Name = "Chiếc", ShortName = "chiếc", IsActive = true },
                    new Unit { Name = "Kg", ShortName = "kg", IsActive = true },
                    new Unit { Name = "Thùng", ShortName = "thùng", IsActive = true },
                    new Unit { Name = "Chai", ShortName = "chai", IsActive = true });
                await db.SaveChangesAsync();
            }

            if (!await db.Categories.AnyAsync())
            {
                db.Categories.AddRange(
                    new Category { Name = "Điện thoại", Description = "Smartphone các loại", IsActive = true, IsTrending = true },
                    new Category { Name = "Laptop", Description = "Máy tính xách tay và MacBook", IsActive = true, IsTrending = true },
                    new Category { Name = "Phụ kiện", Description = "Tai nghe, sạc, cáp, ốp lưng", IsActive = true, IsTrending = false },
                    new Category { Name = "Đồ gia dụng", Description = "Nồi cơm, quạt, máy lọc không khí", IsActive = true, IsTrending = false },
                    new Category { Name = "Thời trang", Description = "Quần áo, giày dép", IsActive = true, IsTrending = true });
                await db.SaveChangesAsync();
            }

            if (!await db.Brands.AnyAsync())
            {
                var brands = new[]
                {
                    "Apple", "Samsung", "Xiaomi", "OPPO", "Vivo", "Dell", "HP", "Asus", "Acer", "Lenovo",
                    "Logitech", "DareU", "Anker", "Sony", "Philips"
                };

                db.Brands.AddRange(brands.Select(name => new Brand { Name = name, IsActive = true }));
                await db.SaveChangesAsync();
            }
        }

        private static async Task<(AdminGroup Super, AdminGroup Warehouse, AdminGroup Sales)> SeedGroupsAndPermissionsAsync(ApplicationDbContext db)
        {
            var superGroup = await EnsureGroupAsync(db, "Super Admin", "Full System Access");
            var warehouseGroup = await EnsureGroupAsync(db, "Warehouse Staff", "Quản lý kho, nhập hàng, sản phẩm");
            var salesGroup = await EnsureGroupAsync(db, "Sales Staff", "Nhân viên kinh doanh");

            if (!await db.GroupPermissions.AnyAsync(p => p.AdminGroupId == superGroup.Id && p.Module == PermissionConstants.Wildcard && p.Action == PermissionConstants.Wildcard))
            {
                db.GroupPermissions.Add(new GroupPermission
                {
                    AdminGroupId = superGroup.Id,
                    Module = PermissionConstants.Wildcard,
                    Action = PermissionConstants.Wildcard
                });
            }

            var warehousePerms = await db.GroupPermissions.Where(p => p.AdminGroupId == warehouseGroup.Id).ToListAsync();
            db.GroupPermissions.RemoveRange(warehousePerms);
            GrantPermissions(
                db,
                warehouseGroup.Id,
                new Dictionary<string, string[]>
                {
                    [PermissionConstants.Modules.Dashboard] = new[] { PermissionConstants.Actions.Show },
                    [PermissionConstants.Modules.Products] = new[] { PermissionConstants.Actions.Show, PermissionConstants.Actions.Create, PermissionConstants.Actions.Edit, PermissionConstants.Actions.Export },
                    [PermissionConstants.Modules.Stock] = new[] { PermissionConstants.Actions.Show, PermissionConstants.Actions.Create, PermissionConstants.Actions.Edit, PermissionConstants.Actions.Export },
                    [PermissionConstants.Modules.Purchases] = new[] { PermissionConstants.Actions.Show, PermissionConstants.Actions.Create, PermissionConstants.Actions.Edit, PermissionConstants.Actions.Export },
                    [PermissionConstants.Modules.Suppliers] = new[] { PermissionConstants.Actions.Show, PermissionConstants.Actions.Create, PermissionConstants.Actions.Edit, PermissionConstants.Actions.Export },
                    [PermissionConstants.Modules.Units] = new[] { PermissionConstants.Actions.Show, PermissionConstants.Actions.Create, PermissionConstants.Actions.Edit, PermissionConstants.Actions.Export },
                    [PermissionConstants.Modules.Categories] = new[] { PermissionConstants.Actions.Show, PermissionConstants.Actions.Create, PermissionConstants.Actions.Edit, PermissionConstants.Actions.Export },
                    [PermissionConstants.Modules.Brands] = new[] { PermissionConstants.Actions.Show, PermissionConstants.Actions.Create, PermissionConstants.Actions.Edit }
                });

            var salesPerms = await db.GroupPermissions.Where(p => p.AdminGroupId == salesGroup.Id).ToListAsync();
            db.GroupPermissions.RemoveRange(salesPerms);
            GrantPermissions(
                db,
                salesGroup.Id,
                new Dictionary<string, string[]>
                {
                    [PermissionConstants.Modules.Dashboard] = new[] { PermissionConstants.Actions.Show },
                    [PermissionConstants.Modules.Customers] = new[] { PermissionConstants.Actions.Show, PermissionConstants.Actions.Create, PermissionConstants.Actions.Edit, PermissionConstants.Actions.Delete, PermissionConstants.Actions.Export },
                    [PermissionConstants.Modules.Invoices] = new[] { PermissionConstants.Actions.Show, PermissionConstants.Actions.Create, PermissionConstants.Actions.Edit, PermissionConstants.Actions.Delete, PermissionConstants.Actions.Export },
                    [PermissionConstants.Modules.Products] = new[] { PermissionConstants.Actions.Show },
                    [PermissionConstants.Modules.Stock] = new[] { PermissionConstants.Actions.Show },
                    [PermissionConstants.Modules.Reports] = new[] { PermissionConstants.Actions.Show, PermissionConstants.Actions.Export }
                });

            await db.SaveChangesAsync();
            return (superGroup, warehouseGroup, salesGroup);
        }

        private static async Task<AdminGroup> EnsureGroupAsync(ApplicationDbContext db, string name, string description)
        {
            var group = await db.AdminGroups.FirstOrDefaultAsync(g => g.Name == name);
            if (group == null)
            {
                group = new AdminGroup { Name = name, Description = description, IsActive = true };
                db.AdminGroups.Add(group);
                await db.SaveChangesAsync();
            }
            else if (!group.IsActive)
            {
                group.IsActive = true;
                group.Description = description;
                await db.SaveChangesAsync();
            }

            return group;
        }

        private static void GrantPermissions(ApplicationDbContext db, int adminGroupId, IDictionary<string, string[]> permissionMap)
        {
            foreach (var entry in permissionMap)
            {
                foreach (var action in entry.Value.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    db.GroupPermissions.Add(new GroupPermission
                    {
                        AdminGroupId = adminGroupId,
                        Module = entry.Key,
                        Action = action
                    });
                }
            }
        }

        private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager, int superGroupId, int warehouseGroupId, int salesGroupId)
        {
            await EnsureUserAsync(userManager, "admin@osms.local", "Admin@12345", "Super Administrator", superGroupId);
            await EnsureUserAsync(userManager, "warehouse@osms.local", "Warehouse@12345", "Trưởng Kho", warehouseGroupId);
            await EnsureUserAsync(userManager, "sales@osms.local", "Sales@12345", "Nhân viên Sales", salesGroupId);
        }

        private static async Task EnsureUserAsync(UserManager<ApplicationUser> userManager, string email, string password, string fullName, int adminGroupId)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName,
                    IsActive = true,
                    EmailConfirmed = true,
                    AdminGroupId = adminGroupId
                };

                var result = await userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(x => x.Description));
                    throw new InvalidOperationException($"Cannot create seed user '{email}': {errors}");
                }
            }
            else
            {
                var changed = false;
                if (!user.IsActive)
                {
                    user.IsActive = true;
                    changed = true;
                }
                if (!user.EmailConfirmed)
                {
                    user.EmailConfirmed = true;
                    changed = true;
                }
                if (user.AdminGroupId != adminGroupId)
                {
                    user.AdminGroupId = adminGroupId;
                    changed = true;
                }
                if (!string.Equals(user.FullName, fullName, StringComparison.Ordinal))
                {
                    user.FullName = fullName;
                    changed = true;
                }

                if (changed)
                {
                    var result = await userManager.UpdateAsync(user);
                    if (!result.Succeeded)
                    {
                        var errors = string.Join("; ", result.Errors.Select(x => x.Description));
                        throw new InvalidOperationException($"Cannot update seed user '{email}': {errors}");
                    }
                }
            }
        }

        private static async Task SeedBusinessDataAsync(ApplicationDbContext db)
        {
            if (!await db.Settings.AnyAsync())
            {
                db.Settings.Add(new Setting
                {
                    CompanyName = "OSMS Store",
                    Currency = "VND",
                    LogoPath = null
                });
                await db.SaveChangesAsync();
            }

            if (!await db.Suppliers.AnyAsync())
            {
                var suppliers = Enumerable.Range(1, 6)
                    .Select(i => new Supplier
                    {
                        Name = $"Nhà cung cấp {i}",
                        Phone = $"098877766{i}",
                        Email = $"supplier{i}@partner.com",
                        Address = i <= 3 ? $"KCN Số {i}, Hà Nội" : $"Khu logistics số {i}, TP.HCM",
                        IsActive = i != 6,
                        CreatedAt = DateTime.UtcNow.AddDays(-i * 7)
                    })
                    .ToList();

                db.Suppliers.AddRange(suppliers);
                await db.SaveChangesAsync();
            }

            if (!await db.Customers.AnyAsync())
            {
                var customers = Enumerable.Range(1, 15)
                    .Select(i => new Customer
                    {
                        Name = $"Khách hàng {i}",
                        Phone = $"090512345{i}",
                        Email = $"customer{i}@gmail.com",
                        Address = i % 3 == 0 ? $"Số {i} đường ABC, Hà Nội" : $"Số {i} đường ABC, TP.HCM",
                        IsActive = i != 15,
                        CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(5, 120))
                    })
                    .ToList();

                db.Customers.AddRange(customers);
                await db.SaveChangesAsync();
            }

            if (!await db.Products.AnyAsync())
            {
                var categoryIds = await db.Categories.Select(x => x.Id).ToListAsync();
                var unitIds = await db.Units.Where(x => x.IsActive).Select(x => x.Id).ToListAsync();
                var brandIds = await db.Brands.Where(x => x.IsActive).Select(x => x.Id).ToListAsync();
                var random = new Random();
                var products = new List<Product>();

                for (int i = 1; i <= 36; i++)
                {
                    var cost = random.Next(10, 500) * 10000m;
                    var sale = cost + cost * random.Next(10, 40) / 100m;
                    products.Add(new Product
                    {
                        SKU = $"SP{i:000}",
                        Name = $"Sản phẩm Test {i}",
                        Description = $"Mô tả chi tiết cho sản phẩm {i}. Hàng chất lượng cao.",
                        Content = $"<p>Mô tả HTML cho sản phẩm {i} dùng để demo phần chi tiết sản phẩm và kiểm thử nội dung hiển thị.</p>",
                        CostPrice = cost,
                        SalePrice = sale,
                        StockOnHand = 0,
                        ReorderLevel = i % 4 == 0 ? 5 : (i % 3 == 0 ? 15 : 10),
                        IsActive = i % 18 != 0,
                        IsTrending = i % 4 == 0,
                        CategoryId = categoryIds[random.Next(categoryIds.Count)],
                        UnitId = unitIds[random.Next(unitIds.Count)],
                        BrandId = brandIds.Count == 0 ? null : brandIds[random.Next(brandIds.Count)],
                        ImagePath = null
                    });
                }

                db.Products.AddRange(products);
                await db.SaveChangesAsync();
            }

            if (!await db.Employees.AnyAsync())
            {
                db.Employees.AddRange(
                    new Employee { Name = "Nguyễn Văn An", Phone = "0903000001", Email = "an@company.local", Address = "TP.HCM", Position = "Sales", Salary = 9000000, IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-120) },
                    new Employee { Name = "Trần Thị Bình", Phone = "0903000002", Email = "binh@company.local", Address = "TP.HCM", Position = "Sales", Salary = 9500000, IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-110) },
                    new Employee { Name = "Lê Minh Châu", Phone = "0903000003", Email = "chau@company.local", Address = "Hà Nội", Position = "Warehouse", Salary = 10000000, IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-200) },
                    new Employee { Name = "Phạm Quốc Dũng", Phone = "0903000004", Email = "dung@company.local", Address = "Đà Nẵng", Position = "Warehouse", Salary = 10500000, IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-180) },
                    new Employee { Name = "Võ Thị Em", Phone = "0903000005", Email = "em@company.local", Address = "TP.HCM", Position = "Accountant", Salary = 12000000, IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-250) },
                    new Employee { Name = "Đặng Hữu Phước", Phone = "0903000006", Email = "phuoc@company.local", Address = "TP.HCM", Position = "Manager", Salary = 18000000, IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-365) });
                await db.SaveChangesAsync();
            }

            if (!await db.Attendances.AnyAsync())
            {
                var random = new Random();
                var employeeIds = await db.Employees.Where(x => x.IsActive).Select(x => x.Id).ToListAsync();
                var today = DateTime.UtcNow.Date;
                var attendances = new List<Attendance>();

                foreach (var employeeId in employeeIds)
                {
                    for (int day = 0; day < 30; day++)
                    {
                        var date = today.AddDays(-day);
                        var roll = random.Next(0, 100);
                        var status = roll switch
                        {
                            < 80 => AttendanceStatus.Present,
                            < 88 => AttendanceStatus.Late,
                            < 95 => AttendanceStatus.Leave,
                            _ => AttendanceStatus.Absent
                        };

                        attendances.Add(new Attendance
                        {
                            EmployeeId = employeeId,
                            Date = date,
                            Status = status,
                            Note = status == AttendanceStatus.Late ? "Đi trễ" : null
                        });
                    }
                }

                db.Attendances.AddRange(attendances);
                await db.SaveChangesAsync();
            }

            if (!await db.Expenses.AnyAsync())
            {
                var random = new Random();
                var now = DateTime.UtcNow;
                var expenses = new List<Expense>();
                var totalSalary = await db.Employees.Where(x => x.IsActive).SumAsync(x => x.Salary);

                expenses.Add(new Expense
                {
                    Title = "Tổng lương nhân viên tháng này",
                    Amount = totalSalary,
                    ExpenseDate = now.AddDays(-3),
                    Note = "Chi phí vận hành: lương",
                    IsActive = true
                });

                foreach (var title in new[]
                {
                    "Tiền điện", "Tiền nước", "Internet & dịch vụ", "Thuê mặt bằng", "Vận chuyển",
                    "Marketing", "Văn phòng phẩm", "Bảo trì thiết bị", "Chi phí khác"
                })
                {
                    expenses.Add(new Expense
                    {
                        Title = title,
                        Amount = random.Next(2, 50) * 100000,
                        ExpenseDate = now.AddDays(-random.Next(1, 30)),
                        Note = "Seed demo",
                        IsActive = true
                    });
                }

                db.Expenses.AddRange(expenses);
                await db.SaveChangesAsync();
            }

            await SeedPurchaseHistoryAsync(db);
            await SeedInvoiceHistoryAsync(db);
            await SeedStockAlertSamplesAsync(db);
        }

        private static async Task SeedPurchaseHistoryAsync(ApplicationDbContext db)
        {
            if (await db.Purchases.AnyAsync())
            {
                return;
            }

            var products = await db.Products.Where(x => x.IsActive).ToListAsync();
            var suppliers = await db.Suppliers.Where(x => x.IsActive).ToListAsync();
            var random = new Random();

            for (int i = 0; i < 24; i++)
            {
                var date = DateTime.UtcNow.AddDays(-random.Next(1, 60));
                var supplier = suppliers[random.Next(suppliers.Count)];
                var status = i < 18 ? PurchaseStatus.Received : (i < 21 ? PurchaseStatus.Draft : PurchaseStatus.Cancelled);

                var purchase = new Purchase
                {
                    PurchaseNo = $"PO-{date:yyyyMMdd}-{random.Next(1000, 9999)}",
                    SupplierId = supplier.Id,
                    PurchaseDate = date,
                    Status = status,
                    Items = new List<PurchaseItem>()
                };

                decimal subTotal = 0;
                foreach (var product in products.OrderBy(_ => random.Next()).Take(random.Next(3, 6)))
                {
                    var qty = random.Next(10, 100);
                    var lineTotal = qty * product.CostPrice;
                    purchase.Items.Add(new PurchaseItem
                    {
                        ProductId = product.Id,
                        Qty = qty,
                        UnitCost = product.CostPrice,
                        LineTotal = lineTotal
                    });
                    subTotal += lineTotal;

                    if (status == PurchaseStatus.Received)
                    {
                        product.StockOnHand += qty;
                        db.StockMovements.Add(new StockMovement
                        {
                            ProductId = product.Id,
                            MovementDate = date,
                            Type = StockMovementType.In,
                            Qty = qty,
                            RefType = "Purchase",
                            Note = $"Nhập hàng theo đơn {purchase.PurchaseNo}"
                        });
                    }
                }

                purchase.SubTotal = subTotal;
                purchase.GrandTotal = subTotal;
                db.Purchases.Add(purchase);
            }

            await db.SaveChangesAsync();
        }

        private static async Task SeedInvoiceHistoryAsync(ApplicationDbContext db)
        {
            if (await db.Invoices.AnyAsync())
            {
                return;
            }

            var products = await db.Products.Where(x => x.IsActive && x.StockOnHand > 0).ToListAsync();
            var customers = await db.Customers.Where(x => x.IsActive).ToListAsync();
            var random = new Random();

            for (int i = 0; i < 60; i++)
            {
                var date = DateTime.UtcNow.AddDays(-random.Next(0, 30));
                var status = i < 24
                    ? InvoiceStatus.Paid
                    : (i < 40 ? InvoiceStatus.PartiallyPaid
                    : (i < 52 ? InvoiceStatus.Unpaid
                    : (i < 56 ? InvoiceStatus.Draft : InvoiceStatus.Cancelled)));

                var invoice = new Invoice
                {
                    InvoiceNo = $"INV-{date:yyyyMMdd}-{random.Next(1000, 9999)}",
                    CustomerId = i % 10 == 0 ? null : customers[random.Next(customers.Count)].Id,
                    InvoiceDate = date,
                    Status = status,
                    Items = new List<InvoiceItem>()
                };

                decimal subTotal = 0;
                var pickedProducts = products.OrderBy(_ => random.Next()).Take(random.Next(1, 4)).ToList();
                foreach (var product in pickedProducts)
                {
                    if (product.StockOnHand <= 0)
                    {
                        continue;
                    }

                    var qty = Math.Min(random.Next(1, 5), product.StockOnHand);
                    if (qty <= 0)
                    {
                        continue;
                    }

                    var lineTotal = qty * product.SalePrice;
                    invoice.Items.Add(new InvoiceItem
                    {
                        ProductId = product.Id,
                        Quantity = qty,
                        UnitPrice = product.SalePrice,
                        LineTotal = lineTotal
                    });
                    subTotal += lineTotal;

                    if (status is InvoiceStatus.Paid or InvoiceStatus.PartiallyPaid or InvoiceStatus.Unpaid)
                    {
                        product.StockOnHand -= qty;
                        db.StockMovements.Add(new StockMovement
                        {
                            ProductId = product.Id,
                            MovementDate = date,
                            Type = StockMovementType.Out,
                            Qty = qty,
                            RefType = "Invoice",
                            Note = $"Xuất bán đơn {invoice.InvoiceNo}"
                        });
                    }
                }

                if (invoice.Items.Count == 0)
                {
                    continue;
                }

                invoice.SubTotal = subTotal;
                invoice.GrandTotal = subTotal;
                invoice.PaidAmount = status switch
                {
                    InvoiceStatus.Paid => subTotal,
                    InvoiceStatus.PartiallyPaid => Math.Round(subTotal * random.Next(30, 80) / 100m, 2),
                    _ => 0m
                };
                db.Invoices.Add(invoice);
            }

            await db.SaveChangesAsync();
        }

        private static async Task SeedStockAlertSamplesAsync(ApplicationDbContext db)
        {
            if (await db.StockMovements.AnyAsync(x => x.Type == StockMovementType.Adjust && x.RefType == "SeedAdjustment"))
            {
                return;
            }

            var alertProducts = await db.Products
                .Where(x => x.IsActive)
                .OrderBy(x => x.Id)
                .Take(5)
                .ToListAsync();

            for (int i = 0; i < alertProducts.Count; i++)
            {
                var product = alertProducts[i];
                var targetStock = i switch
                {
                    0 => 0,
                    1 => Math.Max(1, product.ReorderLevel - 1),
                    2 => product.ReorderLevel,
                    _ => Math.Max(2, product.ReorderLevel - 2)
                };

                var delta = targetStock - product.StockOnHand;
                if (delta == 0)
                {
                    continue;
                }

                product.StockOnHand = targetStock;
                db.StockMovements.Add(new StockMovement
                {
                    ProductId = product.Id,
                    MovementDate = DateTime.UtcNow.AddHours(-(i + 1)),
                    Type = StockMovementType.Adjust,
                    Qty = delta,
                    RefType = "SeedAdjustment",
                    Note = $"Điều chỉnh tồn kho mẫu cho test ngưỡng tồn: {product.SKU}"
                });
            }

            await db.SaveChangesAsync();
        }

        private static async Task ResetBusinessDataAsync(ApplicationDbContext db)
        {
            db.StockMovements.RemoveRange(await db.StockMovements.ToListAsync());

            db.InvoiceItems.RemoveRange(await db.InvoiceItems.ToListAsync());
            db.Invoices.RemoveRange(await db.Invoices.ToListAsync());

            db.PurchaseItems.RemoveRange(await db.PurchaseItems.ToListAsync());
            db.Purchases.RemoveRange(await db.Purchases.ToListAsync());

            db.Attendances.RemoveRange(await db.Attendances.ToListAsync());
            db.Employees.RemoveRange(await db.Employees.ToListAsync());
            db.Expenses.RemoveRange(await db.Expenses.ToListAsync());

            db.Products.RemoveRange(await db.Products.ToListAsync());
            db.Brands.RemoveRange(await db.Brands.ToListAsync());
            db.Categories.RemoveRange(await db.Categories.ToListAsync());
            db.Units.RemoveRange(await db.Units.ToListAsync());
            db.Customers.RemoveRange(await db.Customers.ToListAsync());
            db.Suppliers.RemoveRange(await db.Suppliers.ToListAsync());
            db.Settings.RemoveRange(await db.Settings.ToListAsync());

            await db.SaveChangesAsync();
        }
    }
}
