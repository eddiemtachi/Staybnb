using System.ComponentModel.DataAnnotations;
namespace Staybnb.Models;
public class GuestDocument
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public Booking? Booking { get; set; }
    [Required, StringLength(30)] public string DocumentType { get; set; } = "Passport";
    [Required, StringLength(500)] public string DocumentUrl { get; set; } = "";
    [StringLength(20)] public string Status { get; set; } = "Pending";
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
