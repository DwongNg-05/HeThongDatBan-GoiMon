using System.ComponentModel.DataAnnotations;

namespace HeThongDatBan_GoiMon.Models;

public class MenuChangeLog
{
    public long Id { get; set; }
    public int MenuItemId { get; set; }
    public MenuItem MenuItem { get; set; } = null!;
    public int ActorId { get; set; }
    public ManagerAccount Actor { get; set; } = null!;
    // Keep the name at the time of the change, even if the account is renamed later.
    [MaxLength(100)] public string ActorUserName { get; set; } = "";
    public DateTimeOffset ChangedAtUtc { get; set; }
    [MaxLength(150)] public string OldName { get; set; } = "";
    [MaxLength(150)] public string NewName { get; set; } = "";
    public decimal OldPrice { get; set; }
    public decimal NewPrice { get; set; }
}
