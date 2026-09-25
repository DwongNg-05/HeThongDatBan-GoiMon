using System.ComponentModel.DataAnnotations;

namespace HeThongDatBan_GoiMon.Models;

public class ManagerAccount
{
    public int Id { get; set; }
    [MaxLength(100)] public string UserName { get; set; } = "";
    [MaxLength(100)] public string NormalizedUserName { get; set; } = "";
    [MaxLength(20)] public string PhoneNumber { get; set; } = "";
    [MaxLength(512)] public string PasswordHash { get; set; } = "";
    public bool IsActive { get; set; } = true;
}
