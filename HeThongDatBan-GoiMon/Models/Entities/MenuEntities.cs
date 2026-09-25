namespace HeThongDatBan_GoiMon.Models.Entities;

public sealed class MenuCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
}

public sealed class MenuItem
{
    public int Id { get; set; }
    public int MenuCategoryId { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public int PreparationMinutes { get; set; }
    public bool IsActive { get; set; } = true;
    public MenuCategory MenuCategory { get; set; } = null!;
    public ICollection<MenuChangeLog> ChangeLogs { get; set; } = new List<MenuChangeLog>();
    public ICollection<MenuItemAvailability> Availabilities { get; set; } = new List<MenuItemAvailability>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

public sealed class MenuChangeLog
{
    public long Id { get; set; }
    public int MenuItemId { get; set; }
    public int ActorId { get; set; }
    public string? OldName { get; set; }
    public string? NewName { get; set; }
    public decimal? OldPrice { get; set; }
    public decimal? NewPrice { get; set; }
    public DateTimeOffset ChangedAtUtc { get; set; }
    public MenuItem MenuItem { get; set; } = null!;
    public EmployeeAccount Actor { get; set; } = null!;
}
