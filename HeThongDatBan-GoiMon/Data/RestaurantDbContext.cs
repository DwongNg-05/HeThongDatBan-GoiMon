using HeThongDatBan_GoiMon.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HeThongDatBan_GoiMon.Data;

public sealed class RestaurantDbContext(DbContextOptions<RestaurantDbContext> options) : DbContext(options)
{
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<AppModule> AppModules => Set<AppModule>();
    public DbSet<RoleModulePermission> RoleModulePermissions => Set<RoleModulePermission>();
    public DbSet<EmployeeAccount> EmployeeAccounts => Set<EmployeeAccount>();
    public DbSet<FailedLoginAttempt> FailedLoginAttempts => Set<FailedLoginAttempt>();
    public DbSet<LoginAuditLog> LoginAuditLogs => Set<LoginAuditLog>();
    public DbSet<DiningArea> DiningAreas => Set<DiningArea>();
    public DbSet<RestaurantTable> RestaurantTables => Set<RestaurantTable>();
    public DbSet<BusinessHour> BusinessHours => Set<BusinessHour>();
    public DbSet<MenuCategory> MenuCategories => Set<MenuCategory>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<MenuChangeLog> MenuChangeLogs => Set<MenuChangeLog>();
    public DbSet<MenuItemAvailability> MenuItemAvailabilities => Set<MenuItemAvailability>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<ReservationStatusHistory> ReservationStatusHistories => Set<ReservationStatusHistory>();
    public DbSet<TableStatusHistory> TableStatusHistories => Set<TableStatusHistory>();
    public DbSet<DiningSession> DiningSessions => Set<DiningSession>();
    public DbSet<CustomerOrder> CustomerOrders => Set<CustomerOrder>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderItemStatusHistory> OrderItemStatusHistories => Set<OrderItemStatusHistory>();
    public DbSet<TableMerge> TableMerges => Set<TableMerge>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(x => x.Code).IsUnique();
            entity.Property(x => x.Code).HasMaxLength(30);
            entity.Property(x => x.Name).HasMaxLength(100);
            entity.HasData(
                new Role { Id = 1, Code = "MANAGER", Name = "Quản lý" },
                new Role { Id = 2, Code = "WAITER", Name = "Nhân viên phục vụ" },
                new Role { Id = 3, Code = "KITCHEN", Name = "Bếp" },
                new Role { Id = 4, Code = "CASHIER", Name = "Thu ngân" });
        });

        modelBuilder.Entity<AppModule>(entity =>
        {
            entity.HasIndex(x => x.Code).IsUnique();
            entity.Property(x => x.Code).HasMaxLength(50);
            entity.Property(x => x.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<RoleModulePermission>(entity =>
        {
            entity.HasKey(x => new { x.RoleId, x.AppModuleId });
            entity.Property(x => x.AccessLevel).HasMaxLength(20);
            entity.Property(x => x.Scope).HasMaxLength(100);
            entity.HasOne(x => x.Role).WithMany(x => x.Permissions).HasForeignKey(x => x.RoleId);
            entity.HasOne(x => x.AppModule).WithMany(x => x.Permissions).HasForeignKey(x => x.AppModuleId);
        });

        modelBuilder.Entity<EmployeeAccount>(entity =>
        {
            entity.HasIndex(x => x.UserName).IsUnique();
            entity.HasIndex(x => x.PhoneNumber).IsUnique();
            entity.Property(x => x.UserName).HasMaxLength(50);
            entity.Property(x => x.FullName).HasMaxLength(150);
            entity.Property(x => x.PhoneNumber).HasMaxLength(20);
            entity.Property(x => x.PasswordHash).HasMaxLength(500);
            entity.HasOne(x => x.Role).WithMany(x => x.EmployeeAccounts).HasForeignKey(x => x.RoleId);
        });

        modelBuilder.Entity<FailedLoginAttempt>(entity =>
        {
            entity.HasOne(x => x.EmployeeAccount).WithMany(x => x.FailedLoginAttempts).HasForeignKey(x => x.EmployeeAccountId);
        });

        modelBuilder.Entity<LoginAuditLog>(entity =>
        {
            entity.Property(x => x.IpAddress).HasMaxLength(45);
            entity.HasOne(x => x.EmployeeAccount).WithMany(x => x.LoginAuditLogs).HasForeignKey(x => x.EmployeeAccountId);
        });

        modelBuilder.Entity<DiningArea>(entity =>
        {
            entity.HasIndex(x => x.Name).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<RestaurantTable>(entity =>
        {
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => x.QrToken).IsUnique();
            entity.Property(x => x.Code).HasMaxLength(20);
            entity.Property(x => x.QrToken).HasMaxLength(100);
            entity.Property(x => x.CurrentStatus).HasMaxLength(30);
            entity.HasOne(x => x.DiningArea).WithMany(x => x.RestaurantTables).HasForeignKey(x => x.DiningAreaId);
        });

        modelBuilder.Entity<BusinessHour>(entity => entity.HasIndex(x => x.DayOfWeek).IsUnique());

        modelBuilder.Entity<MenuCategory>(entity =>
        {
            entity.HasIndex(x => x.Name).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<MenuItem>(entity =>
        {
            entity.HasIndex(x => x.Code).IsUnique();
            entity.Property(x => x.Code).HasMaxLength(30);
            entity.Property(x => x.Name).HasMaxLength(150);
            entity.Property(x => x.Price).HasPrecision(18, 0);
            entity.HasOne(x => x.MenuCategory).WithMany(x => x.MenuItems).HasForeignKey(x => x.MenuCategoryId);
        });

        modelBuilder.Entity<MenuChangeLog>(entity =>
        {
            entity.Property(x => x.OldName).HasMaxLength(150);
            entity.Property(x => x.NewName).HasMaxLength(150);
            entity.Property(x => x.OldPrice).HasPrecision(18, 0);
            entity.Property(x => x.NewPrice).HasPrecision(18, 0);
            entity.HasOne(x => x.MenuItem).WithMany(x => x.ChangeLogs).HasForeignKey(x => x.MenuItemId);
            entity.HasOne(x => x.Actor).WithMany().HasForeignKey(x => x.ActorId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MenuItemAvailability>(entity =>
        {
            entity.HasIndex(x => new { x.MenuItemId, x.BusinessDate }).IsUnique();
            entity.HasOne(x => x.MenuItem).WithMany(x => x.Availabilities).HasForeignKey(x => x.MenuItemId);
            entity.HasOne(x => x.UpdatedBy).WithMany().HasForeignKey(x => x.UpdatedById).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasIndex(x => x.ReservationCode).IsUnique();
            entity.HasIndex(x => new { x.RestaurantTableId, x.ReservationStartUtc });
            entity.Property(x => x.ReservationCode).HasMaxLength(30);
            entity.Property(x => x.CustomerName).HasMaxLength(150);
            entity.Property(x => x.CustomerPhone).HasMaxLength(20);
            entity.Property(x => x.CustomerEmail).HasMaxLength(150);
            entity.Property(x => x.Status).HasMaxLength(30);
            entity.Property(x => x.Note).HasMaxLength(500);
            entity.Property(x => x.CancelReason).HasMaxLength(500);
            entity.HasOne(x => x.RestaurantTable).WithMany(x => x.Reservations).HasForeignKey(x => x.RestaurantTableId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ConfirmedBy).WithMany().HasForeignKey(x => x.ConfirmedById).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ReservationStatusHistory>(entity =>
        {
            entity.Property(x => x.Status).HasMaxLength(30);
            entity.Property(x => x.Note).HasMaxLength(500);
            entity.HasOne(x => x.Reservation).WithMany(x => x.StatusHistory).HasForeignKey(x => x.ReservationId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.ChangedBy).WithMany().HasForeignKey(x => x.ChangedById).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TableStatusHistory>(entity =>
        {
            entity.Property(x => x.Status).HasMaxLength(30);
            entity.HasOne(x => x.RestaurantTable).WithMany(x => x.StatusHistory).HasForeignKey(x => x.RestaurantTableId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.ChangedBy).WithMany().HasForeignKey(x => x.ChangedById).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DiningSession>(entity =>
        {
            entity.HasIndex(x => x.PublicSessionToken).IsUnique();
            entity.Property(x => x.PublicSessionToken).HasMaxLength(100);
            entity.Property(x => x.Status).HasMaxLength(30);
            entity.HasOne(x => x.RestaurantTable).WithMany(x => x.DiningSessions).HasForeignKey(x => x.RestaurantTableId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Reservation).WithMany(x => x.DiningSessions).HasForeignKey(x => x.ReservationId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.OpenedBy).WithMany().HasForeignKey(x => x.OpenedById).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CustomerOrder>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasIndex(x => x.OrderNumber).IsUnique();
            entity.Property(x => x.OrderNumber).HasMaxLength(30);
            entity.Property(x => x.Status).HasMaxLength(30);
            entity.HasOne(x => x.DiningSession).WithMany(x => x.Orders).HasForeignKey(x => x.DiningSessionId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.CreatedBy).WithMany().HasForeignKey(x => x.CreatedById).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.Property(x => x.MenuItemName).HasMaxLength(150);
            entity.Property(x => x.UnitPrice).HasPrecision(18, 0);
            entity.Property(x => x.Note).HasMaxLength(500);
            entity.Property(x => x.Status).HasMaxLength(30);
            entity.HasOne(x => x.CustomerOrder).WithMany(x => x.OrderItems).HasForeignKey(x => x.CustomerOrderId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.MenuItem).WithMany(x => x.OrderItems).HasForeignKey(x => x.MenuItemId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderItemStatusHistory>(entity =>
        {
            entity.Property(x => x.Status).HasMaxLength(30);
            entity.HasOne(x => x.OrderItem).WithMany(x => x.StatusHistory).HasForeignKey(x => x.OrderItemId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.ChangedBy).WithMany().HasForeignKey(x => x.ChangedById).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TableMerge>(entity =>
        {
            entity.HasOne(x => x.PrimaryDiningSession).WithMany().HasForeignKey(x => x.PrimaryDiningSessionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SecondaryDiningSession).WithMany().HasForeignKey(x => x.SecondaryDiningSessionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.MergedBy).WithMany().HasForeignKey(x => x.MergedById).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.UnmergedBy).WithMany().HasForeignKey(x => x.UnmergedById).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Shift>(entity =>
        {
            entity.HasIndex(x => x.ShiftCode).IsUnique();
            entity.Property(x => x.ShiftCode).HasMaxLength(30);
            entity.Property(x => x.Status).HasMaxLength(30);
            entity.Property(x => x.ExpectedCashAmount).HasPrecision(18, 0);
            entity.Property(x => x.CountedCashAmount).HasPrecision(18, 0);
            entity.Property(x => x.CashDifferenceAmount).HasPrecision(18, 0);
            entity.Property(x => x.DifferenceReason).HasMaxLength(500);
            entity.HasOne(x => x.StartedBy).WithMany().HasForeignKey(x => x.StartedById).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.EndedBy).WithMany().HasForeignKey(x => x.EndedById).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasIndex(x => x.InvoiceNumber).IsUnique();
            entity.HasIndex(x => x.DiningSessionId).IsUnique();
            entity.Property(x => x.InvoiceNumber).HasMaxLength(30);
            entity.Property(x => x.Status).HasMaxLength(30);
            entity.Property(x => x.SubtotalAmount).HasPrecision(18, 0);
            entity.Property(x => x.DiscountAmount).HasPrecision(18, 0);
            entity.Property(x => x.TotalAmount).HasPrecision(18, 0);
            entity.Property(x => x.DiscountReason).HasMaxLength(500);
            entity.Property(x => x.CancelReason).HasMaxLength(500);
            entity.HasOne(x => x.DiningSession).WithOne().HasForeignKey<Invoice>(x => x.DiningSessionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Shift).WithMany(x => x.Invoices).HasForeignKey(x => x.ShiftId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CancelledBy).WithMany().HasForeignKey(x => x.CancelledById).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InvoiceLine>(entity =>
        {
            entity.Property(x => x.ItemName).HasMaxLength(150);
            entity.Property(x => x.UnitPrice).HasPrecision(18, 0);
            entity.Property(x => x.LineTotalAmount).HasPrecision(18, 0);
            entity.Property(x => x.Note).HasMaxLength(500);
            entity.HasOne(x => x.Invoice).WithMany(x => x.Lines).HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.OrderItem).WithMany().HasForeignKey(x => x.OrderItemId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.Property(x => x.PaymentMethod).HasMaxLength(30);
            entity.Property(x => x.PaidAmount).HasPrecision(18, 0);
            entity.Property(x => x.CashReceivedAmount).HasPrecision(18, 0);
            entity.Property(x => x.ChangeAmount).HasPrecision(18, 0);
            entity.Property(x => x.TransactionReference).HasMaxLength(100);
            entity.HasOne(x => x.Invoice).WithMany(x => x.Payments).HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.CollectedBy).WithMany().HasForeignKey(x => x.CollectedById).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<NotificationLog>(entity =>
        {
            entity.Property(x => x.Channel).HasMaxLength(30);
            entity.Property(x => x.NotificationType).HasMaxLength(50);
            entity.Property(x => x.RecipientAddress).HasMaxLength(150);
            entity.Property(x => x.Subject).HasMaxLength(200);
            entity.Property(x => x.Content).HasMaxLength(2000);
            entity.Property(x => x.Status).HasMaxLength(30);
            entity.HasOne(x => x.RecipientAccount).WithMany().HasForeignKey(x => x.RecipientAccountId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Reservation).WithMany(x => x.Notifications).HasForeignKey(x => x.ReservationId).OnDelete(DeleteBehavior.SetNull);
        });
    }
}
