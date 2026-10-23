using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybnb.Data;

namespace Staybnb.Controllers;
public class HomeController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index(string? city, string? propertyType)
    {
        var q = db.Properties.Include(p => p.Images).Where(p => p.IsActive);
        if (!string.IsNullOrWhiteSpace(city)) q = q.Where(p => p.City.Contains(city));
        if (!string.IsNullOrWhiteSpace(propertyType)) q = q.Where(p => p.PropertyType == propertyType);
        ViewBag.City = city; ViewBag.PropertyType = propertyType;
        return View(await q.OrderByDescending(p => p.CreatedAt).ToListAsync());
    }
    public IActionResult Privacy() => View();
    public IActionResult Error() => View();
}
