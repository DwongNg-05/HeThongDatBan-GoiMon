namespace HeThongDatBan_GoiMon.Models.Entities;

public sealed class DiningArea
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<RestaurantTable> RestaurantTables { get; set; } = new List<RestaurantTable>();
}

public sealed class RestaurantTable
{
    public int Id { get; set; }
    public int DiningAreaId { get; set; }
    public string Code { get; set; } = null!;
    public int Capacity { get; set; }
    public string QrToken { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public string CurrentStatus { get; set; } = "Available";
    public DateTimeOffset StatusUpdatedAtUtc { get; set; }
    public DiningArea DiningArea { get; set; } = null!;
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public ICollection<DiningSession> DiningSessions { get; set; } = new List<DiningSession>();
    public ICollection<TableStatusHistory> StatusHistory { get; set; } = new List<TableStatusHistory>();
}

public sealed class BusinessHour
{
    public int Id { get; set; }
    public int DayOfWeek { get; set; }
    public TimeOnly OpenTime { get; set; }
    public TimeOnly CloseTime { get; set; }
    public bool IsOpen { get; set; } = true;
}
