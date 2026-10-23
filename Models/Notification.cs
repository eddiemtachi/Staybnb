using System.ComponentModel.DataAnnotations;
namespace Staybnb.Models;
public class Notification
{
    public int Id { get; set; }
    [Required] public string UserId { get; set; } = "";
    public ApplicationUser? User { get; set; }
    [Required, StringLength(250)] public string Text { get; set; } = "";
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
