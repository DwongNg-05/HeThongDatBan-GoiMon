namespace HeThongDatBan_GoiMon.Models.Entities;

public sealed class Reservation
{
    public int Id { get; set; }
    public string ReservationCode { get; set; } = null!;
    public string CustomerName { get; set; } = null!;
    public string CustomerPhone { get; set; } = null!;
    public string? CustomerEmail { get; set; }
    public int PartySize { get; set; }
    public DateTimeOffset ReservationStartUtc { get; set; }
    public DateTimeOffset ReservationEndUtc { get; set; }
    public int? RestaurantTableId { get; set; }
    public string Status { get; set; } = "Pending";
    public string? Note { get; set; }
    public int? ConfirmedById { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? CancelledAtUtc { get; set; }
    public string? CancelReason { get; set; }
    public RestaurantTable? RestaurantTable { get; set; }
    public EmployeeAccount? ConfirmedBy { get; set; }
    public ICollection<ReservationStatusHistory> StatusHistory { get; set; } = new List<ReservationStatusHistory>();
    public ICollection<DiningSession> DiningSessions { get; set; } = new List<DiningSession>();
    public ICollection<NotificationLog> Notifications { get; set; } = new List<NotificationLog>();
}

public sealed class ReservationStatusHistory
{
    public long Id { get; set; }
    public int ReservationId { get; set; }
    public string Status { get; set; } = null!;
    public string? Note { get; set; }
    public int? ChangedById { get; set; }
    public DateTimeOffset ChangedAtUtc { get; set; }
    public Reservation Reservation { get; set; } = null!;
    public EmployeeAccount? ChangedBy { get; set; }
}

public sealed class TableStatusHistory
{
    public long Id { get; set; }
    public int RestaurantTableId { get; set; }
    public string Status { get; set; } = null!;
    public int? ChangedById { get; set; }
    public DateTimeOffset ChangedAtUtc { get; set; }
    public RestaurantTable RestaurantTable { get; set; } = null!;
    public EmployeeAccount? ChangedBy { get; set; }
}

public sealed class MenuItemAvailability
{
    public long Id { get; set; }
    public int MenuItemId { get; set; }
    public DateOnly BusinessDate { get; set; }
    public bool IsAvailable { get; set; }
    public int UpdatedById { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public MenuItem MenuItem { get; set; } = null!;
    public EmployeeAccount UpdatedBy { get; set; } = null!;
}

public sealed class DiningSession
{
    public int Id { get; set; }
    public int RestaurantTableId { get; set; }
    public int? ReservationId { get; set; }
    public string PublicSessionToken { get; set; } = null!;
    public int GuestCount { get; set; }
    public string Status { get; set; } = "Open";
    public int? OpenedById { get; set; }
    public DateTimeOffset StartedAtUtc { get; set; }
    public DateTimeOffset? EndedAtUtc { get; set; }
    public RestaurantTable RestaurantTable { get; set; } = null!;
    public Reservation? Reservation { get; set; }
    public EmployeeAccount? OpenedBy { get; set; }
    public ICollection<CustomerOrder> Orders { get; set; } = new List<CustomerOrder>();
}

public sealed class NotificationLog
{
    public long Id { get; set; }
    public int? RecipientAccountId { get; set; }
    public int? ReservationId { get; set; }
    public string Channel { get; set; } = null!;
    public string NotificationType { get; set; } = null!;
    public string RecipientAddress { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string Status { get; set; } = "Pending";
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? SentAtUtc { get; set; }
    public EmployeeAccount? RecipientAccount { get; set; }
    public Reservation? Reservation { get; set; }
}
