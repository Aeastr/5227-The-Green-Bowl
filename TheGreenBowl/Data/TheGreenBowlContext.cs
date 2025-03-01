using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TheGreenBowl.Models;

namespace TheGreenBowl.Data
{
    public class TheGreenBowlContext : IdentityDbContext<ApplicationUser>
    {
        public TheGreenBowlContext(DbContextOptions<TheGreenBowlContext> options)
            : base(options)
        {
        }

        // DbSet properties for all our database tables
        // These allow us to query and manipulate data using LINQ
        public DbSet<tblMenu> tblMenus { get; set; }
        public DbSet<tblMenuItem> tblMenuItems { get; set; }
        public DbSet<tblCategory> tblCategories { get; set; }
        public DbSet<tblMenuCategory> tblMenuCategory { get; set; } // Fixed from tblMenu to tblMenuCategory
        public DbSet<tblMenu_MenuItem> tblMenu_MenuItems { get; set; }
        
        // New DbSets for our basket system
        // These will let us work with user baskets and their items
        public DbSet<tblBasket> tblBaskets { get; set; }
        public DbSet<tblBasketItem> tblBasketItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Call the base implementation first - this sets up the ASP.NET Identity tables
            base.OnModelCreating(modelBuilder);
            
            // Map our entity classes to actual database table names
            // This keeps our code clean with proper C# naming while using simpler DB table names
            modelBuilder.Entity<tblMenu>().ToTable("Menu");
            modelBuilder.Entity<tblMenuItem>().ToTable("MenuItem");
            modelBuilder.Entity<tblCategory>().ToTable("Category");
            modelBuilder.Entity<tblMenuCategory>().ToTable("MenuCategory");
            modelBuilder.Entity<tblMenu_MenuItem>().ToTable("Menu_MenuItem");
            modelBuilder.Entity<tblBasket>().ToTable("Basket"); // New basket table
            modelBuilder.Entity<tblBasketItem>().ToTable("BasketItem"); // New basket items table
            
            // Configure tblMenu
            modelBuilder.Entity<tblMenu>()
                .HasKey(m => m.menuID); // Define primary key

            modelBuilder.Entity<tblMenu>()
                .Property(m => m.name)
                .IsRequired()
                .HasMaxLength(255); // Set maximum length for the name

            modelBuilder.Entity<tblMenu>()
                .Property(m => m.description)
                .HasMaxLength(int.MaxValue); // Optional description (nvarchar(max))

            // Configure tblMenuItem
            modelBuilder.Entity<tblMenuItem>()
                .HasKey(mi => mi.itemID); // Define primary key

            modelBuilder.Entity<tblMenuItem>()
                .Property(mi => mi.name)
                .IsRequired()
                .HasMaxLength(255); // Set maximum length for the name

            modelBuilder.Entity<tblMenuItem>()
                .Property(mi => mi.price)
                .HasColumnType("decimal(10,2)"); // Define price column type

            // Configure tblCategory
            modelBuilder.Entity<tblCategory>()
                .HasKey(c => c.categoryID); // Define primary key

            modelBuilder.Entity<tblCategory>()
                .Property(c => c.name)
                .IsRequired()
                .HasMaxLength(255); // Set maximum length for the name

            modelBuilder.Entity<tblCategory>()
                .Property(c => c.description)
                .HasMaxLength(int.MaxValue); // Optional description (nvarchar(max))

            // Configure tblMenuCategory as the join table
            modelBuilder.Entity<tblMenuCategory>()
                .HasKey(mc => new { mc.menuID, mc.categoryID }); // Composite primary key

            modelBuilder.Entity<tblMenuCategory>()
                .HasOne(mc => mc.Menu) // tblMenuCategory -> tblMenu
                .WithMany(m => m.Categories) // Matches ICollection<tblMenuCategory> in tblMenu
                .HasForeignKey(mc => mc.menuID);

            modelBuilder.Entity<tblMenuCategory>()
                .HasOne(mc => mc.Category) // tblMenuCategory -> tblCategory
                .WithMany(c => c.Menus) // Define the navigation property in tblCategory
                .HasForeignKey(mc => mc.categoryID);

            // Configure tblMenu_MenuItem (existing configuration for menus and items)
            modelBuilder.Entity<tblMenu_MenuItem>()
                .HasKey(mm => new { mm.menuID, mm.itemID }); // Composite primary key

            modelBuilder.Entity<tblMenu_MenuItem>()
                .HasOne(mm => mm.menu) // tblMenu_MenuItem -> tblMenu
                .WithMany(m => m.MenuItems) // Define the navigation property in tblMenu
                .HasForeignKey(mm => mm.menuID);

            modelBuilder.Entity<tblMenu_MenuItem>()
                .HasOne(mm => mm.menuItem) // tblMenu_MenuItem -> tblMenuItem
                .WithMany() // No reverse navigation in tblMenuItem
                .HasForeignKey(mm => mm.itemID);
            
            // Configure tblBasket - our new basket table
            modelBuilder.Entity<tblBasket>()
                .HasKey(b => b.basketID); // Define the primary key

            modelBuilder.Entity<tblBasket>()
                .Property(b => b.createdAt)
                .HasDefaultValueSql("GETDATE()"); // Automatically set creation timestamp

            // Set up the relationship between baskets and users
            // Each basket belongs to one user, and we're not defining a navigation property
            // from user to basket since a user will only have one active basket
            modelBuilder.Entity<tblBasket>()
                .HasOne(b => b.user)
                .WithMany() // No navigation property from user to baskets
                .HasForeignKey(b => b.userID)
                .OnDelete(DeleteBehavior.Cascade); // If user is deleted, delete their basket too

            // Ensure a user can only have one basket at a time
            modelBuilder.Entity<tblBasket>()
                .HasIndex(b => b.userID)
                .IsUnique();

            // Configure tblBasketItem - our basket items join table
            modelBuilder.Entity<tblBasketItem>()
                .HasKey(bi => bi.basketItemID); // Define primary key

            modelBuilder.Entity<tblBasketItem>()
                .Property(bi => bi.quantity)
                .IsRequired()
                .HasDefaultValue(1); // Default quantity is 1 if not specified

            // Set up the relationship between basket items and baskets
            // Each basket item belongs to one basket, and a basket can have many items
            modelBuilder.Entity<tblBasketItem>()
                .HasOne(bi => bi.basket)
                .WithMany(b => b.basketItems) // Links to the basketItems collection in tblBasket
                .HasForeignKey(bi => bi.basketID)
                .OnDelete(DeleteBehavior.Cascade); // If basket is deleted, delete all its items

            // Set up the relationship between basket items and menu items
            // Each basket item references one menu item, but we don't need a navigation
            // property from menu items to basket items
            modelBuilder.Entity<tblBasketItem>()
                .HasOne(bi => bi.menuItem)
                .WithMany() // No navigation property from menuItem to basketItems
                .HasForeignKey(bi => bi.itemID)
                .OnDelete(DeleteBehavior.Cascade); // If menu item is deleted, remove it from baskets
        }
    }
}
