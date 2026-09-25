namespace HeThongDatBan_GoiMon.Services;

using HeThongDatBan_GoiMon.Models;

public interface IMenuItemService
{
    IReadOnlyList<MenuCategory> GetCategories();
    IReadOnlyList<string> GetPredefinedUnits();
    IReadOnlyList<MenuItem> GetAllMenuItems();
    IReadOnlyList<MenuItem> GetActiveMenuItems();
    MenuItem? GetMenuItemById(int id);
    MenuItem CreateMenuItem(MenuItem item);
    bool UpdateItemStatus(int id, bool isActive);
}

public class MenuItemService : IMenuItemService
{
    private readonly List<MenuCategory> _categories = new()
    {
        new MenuCategory { Id = 1, Name = "Món khai vị", SortOrder = 1, IsActive = true },
        new MenuCategory { Id = 2, Name = "Món chính", SortOrder = 2, IsActive = true },
        new MenuCategory { Id = 3, Name = "Món lẩu", SortOrder = 3, IsActive = true },
        new MenuCategory { Id = 4, Name = "Tráng miệng", SortOrder = 4, IsActive = true },
        new MenuCategory { Id = 5, Name = "Đồ uống", SortOrder = 5, IsActive = true }
    };

    private readonly List<string> _predefinedUnits = new()
    {
        "Đĩa", "Phần", "Bát", "Tô", "Nồi", "Ly", "Chai", "Lon", "Con", "Kg", "Suất"
    };

    private readonly List<MenuItem> _menuItems = new();
    private int _nextId = 1;
    private readonly object _lock = new();

    public MenuItemService()
    {
        // Khởi tạo một số món mẫu ban đầu nếu cần
        CreateMenuItem(new MenuItem
        {
            MenuCategoryId = 2,
            Code = "MON-001",
            Name = "Bò né sốt tiêu đen",
            Price = 35000,
            Unit = "Phần",
            Description = "Bò tươi sốt tiêu đen bắp hạt",
            PreparationTimeMinutes = 15,
            IsActive = true
        });
    }

    public IReadOnlyList<MenuCategory> GetCategories() => _categories.Where(c => c.IsActive).OrderBy(c => c.SortOrder).ToList();

    public IReadOnlyList<string> GetPredefinedUnits() => _predefinedUnits;

    public IReadOnlyList<MenuItem> GetAllMenuItems()
    {
        lock (_lock)
        {
            return _menuItems.OrderByDescending(m => m.Id).ToList();
        }
    }

    public IReadOnlyList<MenuItem> GetActiveMenuItems()
    {
        lock (_lock)
        {
            return _menuItems.Where(m => m.IsActive).OrderBy(m => m.MenuCategoryId).ThenBy(m => m.Name).ToList();
        }
    }

    public MenuItem? GetMenuItemById(int id)
    {
        lock (_lock)
        {
            return _menuItems.FirstOrDefault(m => m.Id == id);
        }
    }

    public bool UpdateItemStatus(int id, bool isActive)
    {
        lock (_lock)
        {
            var item = _menuItems.FirstOrDefault(m => m.Id == id);
            if (item == null) return false;
            item.IsActive = isActive;
            return true;
        }
    }

    public MenuItem CreateMenuItem(MenuItem item)
    {
        lock (_lock)
        {
            item.Id = _nextId++;
            if (string.IsNullOrWhiteSpace(item.Code))
            {
                item.Code = $"MON-{item.Id:D3}";
            }
            var category = _categories.FirstOrDefault(c => c.Id == item.MenuCategoryId);
            item.CategoryName = category != null ? category.Name : "Khác";
            item.CreatedAt = DateTime.UtcNow;
            _menuItems.Add(item);
            return item;
        }
    }
}
