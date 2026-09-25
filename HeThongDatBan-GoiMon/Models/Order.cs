namespace HeThongDatBan_GoiMon.Models;

public class OrderItem
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public int MenuItemId { get; set; }
    public string ItemNameSnapshot { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public string Unit { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal LineTotal => UnitPrice * Quantity;
    public string? Note { get; set; }
    public string Status { get; set; } = "Submitted"; // Submitted, Accepted, Preparing, Ready, Served, Cancelled
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}

public class Order
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string TableName { get; set; } = "Bàn 01";
    public string Status { get; set; } = "Open"; // Open, Closed, Cancelled
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<OrderItem> Items { get; set; } = new();

    public decimal TotalAmount => Items.Where(i => i.Status != "Cancelled").Sum(i => i.LineTotal);
    public int TotalItemsCount => Items.Where(i => i.Status != "Cancelled").Sum(i => i.Quantity);
}
