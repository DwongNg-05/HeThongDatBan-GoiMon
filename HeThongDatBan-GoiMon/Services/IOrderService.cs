namespace HeThongDatBan_GoiMon.Services;

using HeThongDatBan_GoiMon.Models;

public interface IOrderService
{
    Order GetCurrentOrder();
    Order? GetOrderById(long id);
    (bool Success, string Message, OrderItem? Item) AddItemToCurrentOrder(int menuItemId, int quantity, string? note);
    bool ClearCurrentOrder();
}

public class OrderService : IOrderService
{
    private readonly IMenuItemService _menuItemService;
    private readonly List<Order> _orders = new();
    private Order _currentOpenOrder;
    private long _nextOrderId = 1;
    private long _nextItemId = 1;
    private readonly object _lock = new();

    public OrderService(IMenuItemService menuItemService)
    {
        _menuItemService = menuItemService;
        _currentOpenOrder = new Order
        {
            Id = _nextOrderId++,
            Code = $"ORD-{DateTime.Now:yyyyMMdd}-001",
            TableName = "Bàn 01",
            Status = "Open",
            CreatedAt = DateTime.UtcNow
        };
        _orders.Add(_currentOpenOrder);
    }

    public Order GetCurrentOrder()
    {
        lock (_lock)
        {
            return _currentOpenOrder;
        }
    }

    public Order? GetOrderById(long id)
    {
        lock (_lock)
        {
            return _orders.FirstOrDefault(o => o.Id == id);
        }
    }

    public (bool Success, string Message, OrderItem? Item) AddItemToCurrentOrder(int menuItemId, int quantity, string? note)
    {
        lock (_lock)
        {
            if (quantity <= 0)
            {
                return (false, "Số lượng phải lớn hơn 0.", null);
            }

            var menuItem = _menuItemService.GetMenuItemById(menuItemId);
            if (menuItem == null)
            {
                return (false, "Món ăn không tồn tại trong hệ thống.", null);
            }

            // AC: Món ngừng bán không gọi được trên màn hình gọi món
            if (!menuItem.IsActive)
            {
                return (false, $"Món \"{menuItem.Name}\" hiện đang ngừng bán, không thể gọi món.", null);
            }

            // Ghi nhận giá bán ngay tại thời điểm gọi món (Snapshot)
            var orderItem = new OrderItem
            {
                Id = _nextItemId++,
                OrderId = _currentOpenOrder.Id,
                MenuItemId = menuItem.Id,
                ItemNameSnapshot = menuItem.Name,
                UnitPrice = menuItem.Price, // Ghi nhận giá tại thời điểm gọi
                Unit = menuItem.Unit,
                Quantity = quantity,
                Note = note?.Trim(),
                Status = "Submitted",
                SubmittedAt = DateTime.UtcNow
            };

            _currentOpenOrder.Items.Add(orderItem);

            return (true, $"Đã thêm {quantity} {menuItem.Unit} \"{menuItem.Name}\" vào order.", orderItem);
        }
    }

    public bool ClearCurrentOrder()
    {
        lock (_lock)
        {
            _currentOpenOrder.Items.Clear();
            return true;
        }
    }
}
