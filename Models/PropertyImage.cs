using System.ComponentModel.DataAnnotations;
namespace Staybnb.Models;
public class PropertyImage
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public Property? Property { get; set; }
    [Required, StringLength(500)] public string ImageUrl { get; set; } = "";
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
