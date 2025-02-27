using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TheGreenBowl.Data;
using TheGreenBowl.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Existing database context registration
builder.Services.AddDbContext<TheGreenBowlContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TheGreenBowlContext")));

// Add Identity services
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<TheGreenBowlContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/AccessDenied";
});

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Menu/Admin", "AdminPolicy");
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
});


var app = builder.Build();

// Seed roles and the base admin user before the app starts handling requests.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    // Define the role and admin user details
    string adminRole = "Admin";
    string adminEmail = "admin@example.com";
    string adminPassword = "SecurePassword123!"; // Please replace with a stronger password

    // Check if the Admin role exists, and create it if it doesn't.
    if (!await roleManager.RoleExistsAsync(adminRole))
    {
        await roleManager.CreateAsync(new IdentityRole(adminRole));
    }

    // Check if an admin user already exists.
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        // Create the admin user
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true  // optional: mark email as confirmed
        };

        var createUserResult = await userManager.CreateAsync(adminUser, adminPassword);
        if (createUserResult.Succeeded)
        {
            // Add to admin role
            await userManager.AddToRoleAsync(adminUser, adminRole);
        }
        else
        {
            // Log or handle errors as needed.
            // For example, write to a log:
            foreach (var error in createUserResult.Errors)
            {
                Console.WriteLine($"Error: {error.Code} - {error.Description}");
            }
        }
    }
    else
    {
        // Make sure the user is in the admin role
        if (!await userManager.IsInRoleAsync(adminUser, adminRole))
        {
            await userManager.AddToRoleAsync(adminUser, adminRole);
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add authentication and authorization middleware.
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
