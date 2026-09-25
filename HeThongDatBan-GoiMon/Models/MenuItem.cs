using System.ComponentModel.DataAnnotations;

namespace HeThongDatBan_GoiMon.Models;

public class MenuItem
{
    public int Id { get; set; }
    [MaxLength(150)] public string Name { get; set; } = "";
    public decimal Price { get; set; }
    [Timestamp] public byte[] RowVersion { get; set; } = [];
}
