using System.ComponentModel.DataAnnotations;
namespace Staybnb.Models;
public class Booking
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public Property? Property { get; set; }
    [Required] public string GuestId { get; set; } = "";
    public ApplicationUser? Guest { get; set; }
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    [Range(1, 30)] public int Travelers { get; set; } = 1;
    public decimal TotalPrice { get; set; }
    [StringLength(20)] public string Status { get; set; } = "Pending";
    public List<GuestDocument> GuestDocuments { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
