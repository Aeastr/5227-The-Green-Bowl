using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TheGreenBowl.Data;
using TheGreenBowl.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();

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
    var dbContext = services.GetRequiredService<TheGreenBowlContext>(); // Get the database context

    // Define the role and admin user details
    string adminRole = "Admin";
    string adminEmail = "admin@example.com";
    string adminPassword = "SecurePassword123!";

    // Check if the Admin role exists, and create it if it doesn't.
    if (!await roleManager.RoleExistsAsync(adminRole))
    {
        await roleManager.CreateAsync(new IdentityRole(adminRole));
    }

    // Check if an admin user already exists.
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    bool isNewUser = false;
    
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
            isNewUser = true; // Mark as a new user so we know to create a basket
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
    
    // Check if the admin user has a basket, and create one if they don't
    // We need to get the user ID first
    string adminUserId = adminUser.Id;
    
    // Check if this user already has a basket
    var existingBasket = await dbContext.tblBaskets
        .FirstOrDefaultAsync(b => b.userID == adminUserId);
    
    if (existingBasket == null)
    {
        // Create a new basket for the admin user
        var adminBasket = new tblBasket
        {
            userID = adminUserId,
            createdAt = DateTime.Now
        };
    
        // Use a separate transaction context to ensure this operation completes
        using (var transaction = await dbContext.Database.BeginTransactionAsync())
        {
            try
            {
                // Add the basket to the database
                dbContext.tblBaskets.Add(adminBasket);
                var saveResult = await dbContext.SaveChangesAsync();
            
                // Check if any rows were affected
                Console.WriteLine($"SaveChanges affected {saveResult} rows");
            
                // Explicitly commit the transaction
                await transaction.CommitAsync();
            
                Console.WriteLine($"Created a new basket for admin user {adminUserId} with ID: {adminBasket.basketID}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving basket: {ex.Message}");
                // No need to roll back as it will happen automatically when the using block exits
                throw;
            }
        }
    }

    else
    {
        Console.WriteLine($"User already has basket");
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
app.MapControllers();

app.Run();
