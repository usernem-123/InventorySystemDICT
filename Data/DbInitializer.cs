using InventorySystem.Models;
using Microsoft.AspNetCore.Identity;

namespace InventorySystem.Data;

public static class DbInitializer
{
    public static void Seed(AppDbContext db)
    {
        db.Database.EnsureCreated();

        if (!db.Users.Any())
        {
            var admin = new User
            {
                Username = "admin",
                FullName = "System Administrator",
                Role = "Admin"
            };

            var hasher = new PasswordHasher<User>();
            admin.PasswordHash = hasher.HashPassword(admin, "root");

            db.Users.Add(admin);
        }

        if (!db.Categories.Any())
        {
            db.Categories.AddRange(
                new Category
                {
                    Name = "ICT",
                    MinimumStock = 1
                },
                new Category
                {
                    Name = "Non-ICT (Office)",
                    MinimumStock = 10
                },
                new Category
                {
                    Name = "Non-ICT (Cleaning)",
                    MinimumStock = 10
                }
            );
        }

        db.SaveChanges();
    }
}