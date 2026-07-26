using InventorySystem.Models;
using Microsoft.AspNetCore.Identity;

namespace InventorySystem.Data;

public static class DbInitializer
{
    public static void Seed(AppDbContext db)
    {
        db.Database.EnsureCreated();

        if(db.Users.Any()) return;

        var admin = new User
        {
          Username = "admin",
          FullName = "System Administrator",
          Role = "Admin"  
        };

        var hasher = new PasswordHasher<User>();

        admin.PasswordHash = hasher.HashPassword(admin, "root");

        db.Users.Add(admin);

        db.SaveChanges();
    }
}