using Microsoft.AspNetCore.Identity;
using Staybnb.Models;
namespace Staybnb.Services;
public class RoleSeeder(RoleManager<IdentityRole> roles, UserManager<ApplicationUser> users, IConfiguration config)
{
    public async Task SeedAsync()
    {
        foreach (var role in new[] { "Guest", "Host", "Admin", "SuperAdmin" })
            if (!await roles.RoleExistsAsync(role)) await roles.CreateAsync(new IdentityRole(role));

        var email = config["SeedAdmin:Email"] ?? "superadmin@staybnb.local";
        var password = config["SeedAdmin:Password"] ?? "ChangeMe123!";
        var admin = await users.FindByEmailAsync(email);
        if (admin == null)
        {
            admin = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true, FullName = "Staybnb SuperAdmin" };
            var result = await users.CreateAsync(admin, password);
            if (!result.Succeeded) throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }
        if (!await users.IsInRoleAsync(admin, "SuperAdmin")) await users.AddToRoleAsync(admin, "SuperAdmin");
    }
}
