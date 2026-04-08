using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineSalesManagementSystem.Domain.Entities;
using AppUser = OnlineSalesManagementSystem.Domain.Entities.ApplicationUser;

namespace OnlineSalesManagementSystem.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<AdminGroup> AdminGroups => Set<AdminGroup>();
    public DbSet<GroupPermission> GroupPermissions => Set<GroupPermission>();

    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Unit> Units => Set<Unit>();

    // ---  chu?n ha dng ny ---
    public DbSet<Brand> Brands => Set<Brand>();
    // -----------------------------

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Purchase> Purchases => Set<Purchase>();
    public DbSet<PurchaseItem> PurchaseItems => Set<PurchaseItem>();

    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();

    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<Setting> Settings => Set<Setting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ApplicationUser
        modelBuilder.Entity<ApplicationUser>(e =>
        {
            e.Property(x => x.FullName).HasMaxLength(150).IsRequired();
            e.Property(x => x.IsActive).HasDefaultValue(true);

            e.HasOne(x => x.AdminGroup)
             .WithMany(g => g.Users)
             .HasForeignKey(x => x.AdminGroupId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // AdminGroup
        modelBuilder.Entity<AdminGroup>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(120).IsRequired();
            e.Property(x => x.IsActive).HasDefaultValue(true);
            e.HasIndex(x => x.Name).IsUnique();
        });

        // GroupPermission
        modelBuilder.Entity<GroupPermission>(e =>
        {
            e.Property(x => x.Module).HasMaxLength(80).IsRequired();
            e.Property(x => x.Action).HasMaxLength(20).IsRequired();

            e.HasIndex(x => new { x.AdminGroupId, x.Module, x.Action }).IsUnique();

            e.HasOne(x => x.AdminGroup)
             .WithMany(g => g.Permissions)
             .HasForeignKey(x => x.AdminGroupId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // Supplier
        modelBuilder.Entity<Supplier>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.Property(x => x.Phone).HasMaxLength(30);
            e.Property(x => x.Email).HasMaxLength(150);
            e.Property(x => x.Address).HasMaxLength(300);
            e.Property(x => x.IsActive).HasDefaultValue(true);
        });

        // Customer
        modelBuilder.Entity<Customer>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.Property(x => x.Phone).HasMaxLength(30);
            e.Property(x => x.Email).HasMaxLength(150);
            e.Property(x => x.Address).HasMaxLength(300);
            e.Property(x => x.IsActive).HasDefaultValue(true);
        });

        // Employee
        modelBuilder.Entity<Employee>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.Property(x => x.Phone).HasMaxLength(30);
            e.Property(x => x.Email).HasMaxLength(150);
            e.Property(x => x.Address).HasMaxLength(300);
            e.Property(x => x.Position).HasMaxLength(120);
            e.Property(x => x.Salary).HasColumnType("decimal(18,2)");
            e.Property(x => x.IsActive).HasDefaultValue(true);
        });

        // Category
        modelBuilder.Entity<Category>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(120).IsRequired();
            e.Property(x => x.IsActive).HasDefaultValue(true);
            e.HasIndex(x => x.Name).IsUnique();
            e.Property(x => x.IsActive).HasDefaultValue(true);
        });

        // Unit
        modelBuilder.Entity<Unit>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(80).IsRequired();
            e.Property(x => x.ShortName).HasMaxLength(20);
            e.Property(x => x.IsActive).HasDefaultValue(true);
            e.HasIndex(x => x.Name).IsUnique();
        });

        // --- C?U HNH CHO BRAND (M?i thm) ---
        modelBuilder.Entity<Brand>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.IsActive).HasDefaultValue(true);
        });
        // -------------------------------------

        // Product
        modelBuilder.Entity<Product>(e =>
        {
            e.Property(x => x.SKU).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.SKU).IsUnique();

            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.CostPrice).HasColumnType("decimal(18,2)");
            e.Property(x => x.SalePrice).HasColumnType("decimal(18,2)");
            e.Property(x => x.StockOnHand).HasDefaultValue(0);
            e.Property(x => x.ReorderLevel).HasDefaultValue(5);
            e.Property(x => x.IsActive).HasDefaultValue(true);
            e.Property(x => x.IsTrending).HasDefaultValue(false);

            e.HasOne(x => x.Category)
             .WithMany()
             .HasForeignKey(x => x.CategoryId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Unit)
             .WithMany()
             .HasForeignKey(x => x.UnitId)
             .OnDelete(DeleteBehavior.Restrict);

            // --- C?U HNH QUAN H? BRAND (M?i thm) ---
            e.HasOne(x => x.Brand)
             .WithMany()
             .HasForeignKey(x => x.BrandId)
             .OnDelete(DeleteBehavior.Restrict); // Khng cho xa Brand n?u c SP dang dng
            // -----------------------------------------
        });

        // Purchase / PurchaseItem
        modelBuilder.Entity<Purchase>(e =>
        {
            e.Property(x => x.PurchaseNo).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.PurchaseNo).IsUnique();

            e.Property(x => x.SubTotal).HasColumnType("decimal(18,2)");
            e.Property(x => x.GrandTotal).HasColumnType("decimal(18,2)");
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);

            e.HasOne(x => x.Supplier)
             .WithMany()
             .HasForeignKey(x => x.SupplierId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PurchaseItem>(e =>
        {
            e.Property(x => x.UnitCost).HasColumnType("decimal(18,2)");
            e.Property(x => x.LineTotal).HasColumnType("decimal(18,2)");

            e.HasOne(x => x.Purchase)
             .WithMany(p => p.Items)
             .HasForeignKey(x => x.PurchaseId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Product)
             .WithMany()
             .HasForeignKey(x => x.ProductId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // Invoice / InvoiceItem
        modelBuilder.Entity<Invoice>(e =>
        {
            e.Property(x => x.InvoiceNo).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.InvoiceNo).IsUnique();

            e.Property(x => x.SubTotal).HasColumnType("decimal(18,2)");
            e.Property(x => x.GrandTotal).HasColumnType("decimal(18,2)");
            e.Property(x => x.PaidAmount).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);

            e.HasOne(x => x.Customer)
             .WithMany()
             .HasForeignKey(x => x.CustomerId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<InvoiceItem>(e =>
        {
            e.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
            e.Property(x => x.LineTotal).HasColumnType("decimal(18,2)");

            e.HasOne(x => x.Invoice)
             .WithMany(i => i.Items)
             .HasForeignKey(x => x.InvoiceId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Product)
             .WithMany()
             .HasForeignKey(x => x.ProductId)
             .OnDelete(DeleteBehavior.Restrict);

            // C?u hnh c?t tnh ton
            e.Property(x => x.LineTotal)
             .HasComputedColumnSql("UnitPrice * Quantity", stored: true)
             .ValueGeneratedOnAddOrUpdate();
        });

        // Expense
        modelBuilder.Entity<Expense>(e =>
        {
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            e.Property(x => x.IsActive).HasDefaultValue(true);
        });

        // Attendance
        modelBuilder.Entity<Attendance>(e =>
        {
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);

            e.HasOne(x => x.Employee)
             .WithMany()
             .HasForeignKey(x => x.EmployeeId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => new { x.EmployeeId, x.Date }).IsUnique();
        });

        // StockMovement
        modelBuilder.Entity<StockMovement>(e =>
        {
            e.Property(x => x.Type).HasConversion<string>().HasMaxLength(30);
            e.Property(x => x.RefType).HasMaxLength(50);

            e.HasOne(x => x.Product)
             .WithMany()
             .HasForeignKey(x => x.ProductId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // Setting
        modelBuilder.Entity<Setting>(e =>
        {
            e.Property(x => x.CompanyName).HasMaxLength(200).IsRequired();
            e.Property(x => x.Currency).HasMaxLength(10).HasDefaultValue("VND");
        });
    }
}
 
