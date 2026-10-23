using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybnb.Data;
using Staybnb.Models;
using Staybnb.Services;

namespace Staybnb.Controllers;

[Authorize(Roles = "Guest")]
public class HostController(ApplicationDbContext db, UserManager<ApplicationUser> users, ActivityLogger logger) : Controller
{
    [HttpGet] public IActionResult Apply() => View(new HostApplication());
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(HostApplication model)
    {
        if (string.IsNullOrWhiteSpace(model.ImageUrl))
            ModelState.AddModelError(nameof(model.ImageUrl), "At least one property image URL is required.");
        if (!ModelState.IsValid) return View(model);
        model.UserId = users.GetUserId(User)!;
        model.Status = "Pending";
        db.HostApplications.Add(model);
        await db.SaveChangesAsync();
        await logger.LogAsync(model.UserId, "HostApplicationSubmitted", $"Application #{model.Id}");
        TempData["Message"] = "Application submitted for review.";
        return RedirectToAction("Index", "Home");
    }
}

[Authorize(Roles = "Host")]
public class HostDashboardController(ApplicationDbContext db, UserManager<ApplicationUser> users, ActivityLogger logger) : Controller
{
    public async Task<IActionResult> Index()
    {
        var id = users.GetUserId(User);
        return View(await db.Properties.Include(p => p.Images).Where(p => p.HostId == id).ToListAsync());
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var p = await db.Properties.FirstOrDefaultAsync(x => x.Id == id && x.HostId == users.GetUserId(User));
        if (p == null) return NotFound();
        p.IsActive = !p.IsActive; await db.SaveChangesAsync();
        await logger.LogAsync(p.HostId, "PropertyStatusChanged", $"Property #{id}; active={p.IsActive}");
        return RedirectToAction(nameof(Index));
    }
    public async Task<IActionResult> Bookings()
    {
        var id = users.GetUserId(User);
        return View(await db.Bookings.Include(b => b.Property).Include(b => b.Guest).Where(b => b.Property!.HostId == id).OrderByDescending(b => b.CreatedAt).ToListAsync());
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateBooking(int id, string status)
    {
        if (status is not ("Approved" or "Rejected" or "CheckedIn")) return BadRequest();
        var b = await db.Bookings.Include(x => x.Property).FirstOrDefaultAsync(x => x.Id == id && x.Property!.HostId == users.GetUserId(User));
        if (b == null) return NotFound();
        b.Status = status; await db.SaveChangesAsync();
        db.Notifications.Add(new Notification { UserId = b.GuestId, Text = $"Booking #{b.Id}: {status}" });
        await db.SaveChangesAsync();
        await logger.LogAsync(users.GetUserId(User), "BookingStatusUpdated", $"Booking #{id}: {status}");
        return RedirectToAction(nameof(Bookings));
    }
    [HttpGet] public async Task<IActionResult> CheckInProcess(int propertyId)
    {
        var p = await db.Properties.FirstOrDefaultAsync(x => x.Id == propertyId && x.HostId == users.GetUserId(User));
        if (p == null) return NotFound();
        var model = await db.CheckInProcesses.FirstOrDefaultAsync(x => x.PropertyId == propertyId) ?? new CheckInProcess { PropertyId = propertyId };
        return View(model);
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckInProcess(CheckInProcess model)
    {
        var p = await db.Properties.FirstOrDefaultAsync(x => x.Id == model.PropertyId && x.HostId == users.GetUserId(User));
        if (p == null) return NotFound();
        var existing = await db.CheckInProcesses.FirstOrDefaultAsync(x => x.PropertyId == model.PropertyId);
        if (existing == null) db.CheckInProcesses.Add(model);
        else { existing.Rules = model.Rules; existing.RequiredDocuments = model.RequiredDocuments; existing.UpdatedAt = DateTime.UtcNow; }
        await db.SaveChangesAsync(); return RedirectToAction(nameof(Index));
    }
}
