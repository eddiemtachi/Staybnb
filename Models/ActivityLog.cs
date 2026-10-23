using System.ComponentModel.DataAnnotations;
namespace Staybnb.Models;
public class ActivityLog
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    [Required, StringLength(100)] public string Action { get; set; } = "";
    [StringLength(1000)] public string? Details { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
