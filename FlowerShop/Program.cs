using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FlowerShop.Data;
using FlowerShop.Areas.Identity.Data;
using Amazon.XRay.Recorder.Handlers.AwsSdk;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("FlowerShopContextConnection") ?? throw new InvalidOperationException("Connection string 'FlowerShopContextConnection' not found.");

builder.Services.AddDbContext<FlowerShopContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<FlowerShopUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<FlowerShopContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
AWSSDKHandler.RegisterXRayForAllServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseXRay("MVCFlowerShop System");
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
//order must be autentication first before authorization coz login first then authorize
app.UseAuthentication();    //direct login page
app.UseAuthorization(); //check customer permission


//the first page what you will show the default thing after typing the url
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
