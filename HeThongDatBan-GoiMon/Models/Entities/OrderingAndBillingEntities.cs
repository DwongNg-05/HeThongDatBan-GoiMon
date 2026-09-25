namespace HeThongDatBan_GoiMon.Models.Entities;

public sealed class CustomerOrder
{
    public long Id { get; set; }
    public int DiningSessionId { get; set; }
    public string OrderNumber { get; set; } = null!;
    public string Status { get; set; } = "Submitted";
    public int? CreatedById { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DiningSession DiningSession { get; set; } = null!;
    public EmployeeAccount? CreatedBy { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

public sealed class OrderItem
{
    public long Id { get; set; }
    public long CustomerOrderId { get; set; }
    public int MenuItemId { get; set; }
    public string MenuItemName { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public string? Note { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTimeOffset StatusUpdatedAtUtc { get; set; }
    public DateTimeOffset? ServedAtUtc { get; set; }
    public CustomerOrder CustomerOrder { get; set; } = null!;
    public MenuItem MenuItem { get; set; } = null!;
    public ICollection<OrderItemStatusHistory> StatusHistory { get; set; } = new List<OrderItemStatusHistory>();
}

public sealed class OrderItemStatusHistory
{
    public long Id { get; set; }
    public long OrderItemId { get; set; }
    public string Status { get; set; } = null!;
    public int? ChangedById { get; set; }
    public DateTimeOffset ChangedAtUtc { get; set; }
    public OrderItem OrderItem { get; set; } = null!;
    public EmployeeAccount? ChangedBy { get; set; }
}

public sealed class TableMerge
{
    public long Id { get; set; }
    public int PrimaryDiningSessionId { get; set; }
    public int SecondaryDiningSessionId { get; set; }
    public int MergedById { get; set; }
    public DateTimeOffset MergedAtUtc { get; set; }
    public DateTimeOffset? UnmergedAtUtc { get; set; }
    public int? UnmergedById { get; set; }
    public DiningSession PrimaryDiningSession { get; set; } = null!;
    public DiningSession SecondaryDiningSession { get; set; } = null!;
    public EmployeeAccount MergedBy { get; set; } = null!;
    public EmployeeAccount? UnmergedBy { get; set; }
}

public sealed class Shift
{
    public int Id { get; set; }
    public string ShiftCode { get; set; } = null!;
    public int StartedById { get; set; }
    public DateTimeOffset StartedAtUtc { get; set; }
    public int? EndedById { get; set; }
    public DateTimeOffset? EndedAtUtc { get; set; }
    public string Status { get; set; } = "Open";
    public decimal ExpectedCashAmount { get; set; }
    public decimal? CountedCashAmount { get; set; }
    public decimal? CashDifferenceAmount { get; set; }
    public string? DifferenceReason { get; set; }
    public EmployeeAccount StartedBy { get; set; } = null!;
    public EmployeeAccount? EndedBy { get; set; }
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}

public sealed class Invoice
{
    public long Id { get; set; }
    public int DiningSessionId { get; set; }
    public int? ShiftId { get; set; }
    public string InvoiceNumber { get; set; } = null!;
    public string Status { get; set; } = "Draft";
    public decimal SubtotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? DiscountReason { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? FinalizedAtUtc { get; set; }
    public int? CancelledById { get; set; }
    public DateTimeOffset? CancelledAtUtc { get; set; }
    public string? CancelReason { get; set; }
    public DiningSession DiningSession { get; set; } = null!;
    public Shift? Shift { get; set; }
    public EmployeeAccount? CancelledBy { get; set; }
    public ICollection<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

public sealed class InvoiceLine
{
    public long Id { get; set; }
    public long InvoiceId { get; set; }
    public long? OrderItemId { get; set; }
    public string ItemName { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotalAmount { get; set; }
    public string? Note { get; set; }
    public bool IsChargeableAfterCancellation { get; set; }
    public Invoice Invoice { get; set; } = null!;
    public OrderItem? OrderItem { get; set; }
}

public sealed class Payment
{
    public long Id { get; set; }
    public long InvoiceId { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public decimal PaidAmount { get; set; }
    public decimal? CashReceivedAmount { get; set; }
    public decimal? ChangeAmount { get; set; }
    public string? TransactionReference { get; set; }
    public int CollectedById { get; set; }
    public DateTimeOffset PaidAtUtc { get; set; }
    public Invoice Invoice { get; set; } = null!;
    public EmployeeAccount CollectedBy { get; set; } = null!;
}
