using System.ComponentModel.DataAnnotations;

namespace Staybnb.Models;

public class Property
{
    public int Id { get; set; }
    [Required, StringLength(120)] public string Title { get; set; } = "";
    [Required, StringLength(2000)] public string Description { get; set; } = "";
    [Required, StringLength(100)] public string Address { get; set; } = "";
    [Required, StringLength(80)] public string City { get; set; } = "";
    [Required, StringLength(60)] public string PropertyType { get; set; } = "Apartment";
    [Range(1, 1000000)] public decimal PricePerNight { get; set; }
    [Range(0, 100000)] public decimal CleaningFee { get; set; }
    [Range(0, 100000)] public decimal ServiceFee { get; set; }
    public bool IsActive { get; set; } = true;
    [Required] public string HostId { get; set; } = "";
    public ApplicationUser? Host { get; set; }
    public List<PropertyImage> Images { get; set; } = new();
    public List<Booking> Bookings { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
