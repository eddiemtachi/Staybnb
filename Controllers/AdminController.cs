using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybnb.Data;
using Staybnb.Models;
using Staybnb.Services;

namespace Staybnb.Controllers;

[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminController(ApplicationDbContext db, UserManager<ApplicationUser> users, ActivityLogger logger) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewBag.PendingApplications = await db.HostApplications.CountAsync(a => a.Status == "Pending");
        ViewBag.PendingBookings = await db.Bookings.CountAsync(b => b.Status == "Pending");
        return View(await db.ActivityLogs.OrderByDescending(a => a.CreatedAt).Take(100).ToListAsync());
    }
    public async Task<IActionResult> Applications() => View(await db.HostApplications.Include(a => a.User).OrderBy(a => a.SubmittedAt).ToListAsync());
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ReviewApplication(int id, string decision)
    {
        if (decision is not ("Approved" or "Rejected")) return BadRequest();
        var app = await db.HostApplications.FirstOrDefaultAsync(a => a.Id == id && a.Status == "Pending");
        if (app == null) return NotFound();
        app.Status = decision; app.ReviewedBy = users.GetUserId(User); app.ReviewedAt = DateTime.UtcNow;
        if (decision == "Approved")
        {
            var p = new Property { Title = app.Title, Description = app.Description, Address = app.Address, City = app.City, PropertyType = app.PropertyType, PricePerNight = app.PricePerNight, CleaningFee = app.CleaningFee, ServiceFee = app.ServiceFee, HostId = app.UserId, IsActive = true };
            p.Images.Add(new PropertyImage { ImageUrl = app.ImageUrl });
            db.Properties.Add(p);
            var user = await users.FindByIdAsync(app.UserId);
            if (user != null)
            {
                if (await users.IsInRoleAsync(user, "Guest")) await users.RemoveFromRoleAsync(user, "Guest");
                if (!await users.IsInRoleAsync(user, "Host")) await users.AddToRoleAsync(user, "Host");
            }
        }
        await db.SaveChangesAsync();
        await db.Notifications.AddAsync(new Notification { UserId = app.UserId, Text = $"Host application {decision.ToLower()}." });
        await db.SaveChangesAsync();
        await logger.LogAsync(users.GetUserId(User), "HostApplicationReviewed", $"Application #{id}: {decision}");
        return RedirectToAction(nameof(Applications));
    }
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Users() => View(await users.Users.OrderBy(u => u.Email).ToListAsync());
    [HttpPost, Authorize(Roles = "SuperAdmin"), ValidateAntiForgeryToken]
    public async Task<IActionResult> PromoteToAdmin(string userId)
    {
        var user = await users.FindByIdAsync(userId);
        if (user == null) return NotFound();
        if (await users.IsInRoleAsync(user, "Guest")) await users.RemoveFromRoleAsync(user, "Guest");
        if (!await users.IsInRoleAsync(user, "Admin")) await users.AddToRoleAsync(user, "Admin");
        await logger.LogAsync(users.GetUserId(User), "RoleChanged", $"User {user.Email} promoted to Admin");
        return RedirectToAction(nameof(Users));
    }
    public async Task<IActionResult> Documents() => View(await db.GuestDocuments.Include(d => d.Booking).ThenInclude(b => b!.Property).OrderByDescending(d => d.UploadedAt).ToListAsync());
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ReviewDocument(int id, string status)
    {
        if (status is not ("Verified" or "Rejected")) return BadRequest();
        var d = await db.GuestDocuments.Include(x => x.Booking).ThenInclude(b => b!.Property).FirstOrDefaultAsync(x => x.Id == id);
        if (d == null) return NotFound();
        d.Status = status; await db.SaveChangesAsync();
        await logger.LogAsync(users.GetUserId(User), "GuestDocumentReviewed", $"Document #{id}: {status}");
        return RedirectToAction(nameof(Documents));
    }
    public async Task<IActionResult> Messages()
    {
        return View(await db.Messages.Include(m => m.Sender).Include(m => m.Recipient).OrderByDescending(m => m.SentAt).ToListAsync());
    }
}
