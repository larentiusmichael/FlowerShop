using FlowerShop.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.Data;

public class FlowerShopContext : IdentityDbContext<FlowerShopUser>
{
    public FlowerShopContext(DbContextOptions<FlowerShopContext> options)
        : base(options)
    {
    }

    public DbSet<FlowerShop.Models.Flower>FlowerTable { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
    }
}
