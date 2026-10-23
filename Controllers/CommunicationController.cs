using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybnb.Data;
using Staybnb.Models;

namespace Staybnb.Controllers;

[Authorize]
public class CommunicationController(ApplicationDbContext db, UserManager<ApplicationUser> users) : Controller
{
    public async Task<IActionResult> Inbox()
    {
        var id = users.GetUserId(User);
        var messages = await db.Messages.Include(m => m.Sender).Include(m => m.Recipient)
            .Where(m => m.RecipientId == id || m.SenderId == id).OrderByDescending(m => m.SentAt).ToListAsync();
        foreach (var m in messages.Where(m => m.RecipientId == id && !m.IsRead)) m.IsRead = true;
        await db.SaveChangesAsync();
        return View(messages);
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(string recipientId, string body)
    {
        if (string.IsNullOrWhiteSpace(recipientId) || string.IsNullOrWhiteSpace(body) || body.Length > 2000)
        { TempData["Error"] = "Recipient and message (maximum 2000 characters) are required."; return RedirectToAction(nameof(Inbox)); }
        var recipient = await users.FindByIdAsync(recipientId);
        if (recipient == null) return NotFound();
        var senderId = users.GetUserId(User)!;
        db.Messages.Add(new Message { SenderId = senderId, RecipientId = recipientId, Body = body.Trim() });
        db.Notifications.Add(new Notification { UserId = recipientId, Text = $"New message from {User.Identity?.Name}" });
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Inbox));
    }
    public async Task<IActionResult> Notifications()
    {
        var id = users.GetUserId(User);
        var list = await db.Notifications.Where(n => n.UserId == id).OrderByDescending(n => n.CreatedAt).ToListAsync();
        foreach (var n in list) n.IsRead = true;
        await db.SaveChangesAsync();
        return View(list);
    }
}
