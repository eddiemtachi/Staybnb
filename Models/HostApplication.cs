using System.ComponentModel.DataAnnotations;
namespace Staybnb.Models;
public class HostApplication
{
    public int Id { get; set; }
    [Required] public string UserId { get; set; } = "";
    public ApplicationUser? User { get; set; }
    [Required, StringLength(120)] public string Title { get; set; } = "";
    [Required, StringLength(2000)] public string Description { get; set; } = "";
    [Required, StringLength(100)] public string Address { get; set; } = "";
    [Required, StringLength(80)] public string City { get; set; } = "";
    [Required, StringLength(60)] public string PropertyType { get; set; } = "Apartment";
    [Range(1, 1000000)] public decimal PricePerNight { get; set; }
    [Range(0, 100000)] public decimal CleaningFee { get; set; }
    [Range(0, 100000)] public decimal ServiceFee { get; set; }
    [Required, StringLength(500)] public string ImageUrl { get; set; } = "";
    [StringLength(20)] public string Status { get; set; } = "Pending";
    public string? ReviewedBy { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
}
