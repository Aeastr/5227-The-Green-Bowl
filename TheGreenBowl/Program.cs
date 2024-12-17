using Microsoft.EntityFrameworkCore;
using TheGreenBowl.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Register your DbContext and SQL Server connection
builder.Services.AddDbContext<TheGreenBowlContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TheGreenBowlContext")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();