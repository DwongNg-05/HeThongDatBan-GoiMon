namespace HeThongDatBan_GoiMon.Models;

public class FailedLoginAttempt
{
    public long Id { get; set; }
    public int AccountId { get; set; }
    public ManagerAccount Account { get; set; } = null!;
    public DateTimeOffset OccurredAtUtc { get; set; }
}
