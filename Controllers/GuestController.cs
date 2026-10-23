using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybnb.Data;
using Staybnb.Models;
using Staybnb.Services;

namespace Staybnb.Controllers;

[Authorize(Roles = "Guest")]
public class GuestController(ApplicationDbContext db, UserManager<ApplicationUser> users, ActivityLogger logger) : Controller
{
    public async Task<IActionResult> Bookings()
    {
        var id = users.GetUserId(User);
        return View(await db.Bookings.Include(b => b.Property).Where(b => b.GuestId == id).OrderByDescending(b => b.CreatedAt).ToListAsync());
    }
    [HttpGet] public async Task<IActionResult> Book(int id)
    {
        var p = await db.Properties.Include(x => x.Images).FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
        if (p == null) return NotFound();
        ViewBag.Property = p;
        return View(new Booking { PropertyId = id, CheckIn = DateTime.Today.AddDays(1), CheckOut = DateTime.Today.AddDays(2), Travelers = 1 });
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Book(Booking model)
    {
        var p = await db.Properties.FirstOrDefaultAsync(x => x.Id == model.PropertyId && x.IsActive);
        if (p == null) return NotFound();
        if (model.CheckIn.Date < DateTime.Today || model.CheckOut.Date <= model.CheckIn.Date)
            ModelState.AddModelError("", "Choose a future check-in date and a check-out date after check-in.");
        if (!ModelState.IsValid) { ViewBag.Property = p; return View(model); }
        var nights = (model.CheckOut.Date - model.CheckIn.Date).Days;
        model.GuestId = users.GetUserId(User)!;
        model.TotalPrice = p.PricePerNight * nights + p.CleaningFee + p.ServiceFee;
        model.Status = "Pending";
        db.Bookings.Add(model);
        await db.SaveChangesAsync();
        db.Notifications.Add(new Notification { UserId = p.HostId, Text = $"New booking request #{model.Id}" });
        await db.SaveChangesAsync();
        await logger.LogAsync(model.GuestId, "BookingCreated", $"Booking #{model.Id}; total {model.TotalPrice}");
        return RedirectToAction(nameof(Bookings));
    }
    [HttpGet] public async Task<IActionResult> UploadDocument(int bookingId)
    {
        var b = await db.Bookings.FirstOrDefaultAsync(x => x.Id == bookingId && x.GuestId == users.GetUserId(User) && x.Status == "Approved");
        if (b == null) return NotFound();
        return View(new GuestDocument { BookingId = bookingId });
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadDocument(GuestDocument model)
    {
        var b = await db.Bookings.Include(x => x.Property).FirstOrDefaultAsync(x => x.Id == model.BookingId && x.GuestId == users.GetUserId(User) && x.Status == "Approved");
        if (b == null) return NotFound();
        if (!ModelState.IsValid) return View(model);
        db.GuestDocuments.Add(model); await db.SaveChangesAsync();
        db.Notifications.Add(new Notification { UserId = b.Property!.HostId, Text = $"Guest document submitted for booking #{b.Id}" });
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Bookings));
    }
}
