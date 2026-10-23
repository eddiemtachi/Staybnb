using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Staybnb.Models;

namespace Staybnb.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<PropertyImage> PropertyImages => Set<PropertyImage>();
    public DbSet<HostApplication> HostApplications => Set<HostApplication>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<GuestDocument> GuestDocuments => Set<GuestDocument>();
    public DbSet<CheckInProcess> CheckInProcesses => Set<CheckInProcess>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Property>().Property(p => p.PricePerNight).HasPrecision(12, 2);
        builder.Entity<Property>().Property(p => p.CleaningFee).HasPrecision(12, 2);
        builder.Entity<Property>().Property(p => p.ServiceFee).HasPrecision(12, 2);
        builder.Entity<Booking>().Property(p => p.TotalPrice).HasPrecision(12, 2);
        builder.Entity<PropertyImage>().HasOne(i => i.Property).WithMany(p => p.Images).HasForeignKey(i => i.PropertyId).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<HostApplication>().HasOne(a => a.User).WithMany().HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Booking>().HasOne(b => b.Property).WithMany(p => p.Bookings).HasForeignKey(b => b.PropertyId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Booking>().HasOne(b => b.Guest).WithMany().HasForeignKey(b => b.GuestId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<GuestDocument>().HasOne(d => d.Booking).WithMany(b => b.GuestDocuments).HasForeignKey(d => d.BookingId).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<CheckInProcess>().HasOne(c => c.Property).WithMany().HasForeignKey(c => c.PropertyId).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<Message>().HasOne(m => m.Sender).WithMany().HasForeignKey(m => m.SenderId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Message>().HasOne(m => m.Recipient).WithMany().HasForeignKey(m => m.RecipientId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Notification>().HasOne(n => n.User).WithMany().HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
