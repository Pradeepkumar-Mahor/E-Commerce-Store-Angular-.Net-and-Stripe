using Core.Entities;
using Microsoft.AspNetCore.Identity;
using System.Reflection;
using System.Text.Json;

namespace Infrastructure.Data;

public class StoreContextSeed
{
    public static async Task SeedAsync(StoreContext context)
    {
        //var userManager =UserManager<IdentityUser>();

        //   if (!userManager.Users.Any(x => x.UserName == "admin@test.com"))
        //   {
        //       IdentityUser user = new()
        //       {
        //           UserName = "admin@test.com",
        //           Email = "admin@test.com",
        //       };

        //       _ = await userManager.CreateAsync(user, "Pa$$w0rd");
        //       _ = await userManager.AddToRoleAsync(user, "Admin");
        //   }

        string? path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        if (!context.Products.Any())
        {
            string productsData = await File
                .ReadAllTextAsync(path + @"/Data/SeedData/products.json");

            List<Product>? products = JsonSerializer.Deserialize<List<Product>>(productsData);

            if (products == null) return;

            context.Products.AddRange(products);

            _ = await context.SaveChangesAsync();
        }

        if (!context.DeliveryMethods.Any())
        {
            string dmData = await File
                .ReadAllTextAsync(path + @"/Data/SeedData/delivery.json");

            List<DeliveryMethod>? methods = JsonSerializer.Deserialize<List<DeliveryMethod>>(dmData);

            if (methods == null) return;

            context.DeliveryMethods.AddRange(methods);

            _ = await context.SaveChangesAsync();
        }
    }
}