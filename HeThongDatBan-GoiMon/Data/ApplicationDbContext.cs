using HeThongDatBan_GoiMon.Models;
using Microsoft.EntityFrameworkCore;

namespace HeThongDatBan_GoiMon.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<ManagerAccount> ManagerAccounts => Set<ManagerAccount>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<MenuChangeLog> MenuChangeLogs => Set<MenuChangeLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ManagerAccount>().HasIndex(x => x.NormalizedUserName).IsUnique();
        modelBuilder.Entity<ManagerAccount>().HasIndex(x => x.PhoneNumber).IsUnique();
        modelBuilder.Entity<MenuItem>().Property(x => x.Price).HasPrecision(18, 2);
        modelBuilder.Entity<MenuItem>().ToTable(t => t.HasCheckConstraint("CK_MenuItems_Price", "[Price] >= 0"));
        modelBuilder.Entity<MenuChangeLog>().Property(x => x.OldPrice).HasPrecision(18, 2);
        modelBuilder.Entity<MenuChangeLog>().Property(x => x.NewPrice).HasPrecision(18, 2);
        modelBuilder.Entity<MenuChangeLog>().HasOne(x => x.Actor).WithMany()
            .HasForeignKey(x => x.ActorId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<MenuChangeLog>().HasOne(x => x.MenuItem).WithMany()
            .HasForeignKey(x => x.MenuItemId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<MenuChangeLog>().HasIndex(x => x.ChangedAtUtc);
    }
}
