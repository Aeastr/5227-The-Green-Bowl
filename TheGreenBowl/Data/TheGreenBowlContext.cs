using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TheGreenBowl.Models;

namespace TheGreenBowl.Data
{
    public class TheGreenBowlContext : IdentityDbContext<ApplicationUser> // Changed inheritance
    {
        public TheGreenBowlContext(DbContextOptions<TheGreenBowlContext> options)
            : base(options)
        {
        }

        // Existing DbSets
        public DbSet<tblMenu> tblMenus { get; set; }
        public DbSet<tblMenuItem> tblMenuItems { get; set; }
        public DbSet<tblCategory> tblCategories { get; set; }
        public DbSet<tblMenu> tblMenuCategory { get; set; }
        public DbSet<tblMenu_MenuItem> tblMenu_MenuItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // This is crucial for Identity
            
            modelBuilder.Entity<tblMenu>().ToTable("Menu");
            modelBuilder.Entity<tblMenuItem>().ToTable("MenuItem");
            modelBuilder.Entity<tblCategory>().ToTable("Category");
            modelBuilder.Entity<tblMenuCategory>().ToTable("MenuCategory");
            modelBuilder.Entity<tblMenu_MenuItem>().ToTable("Menu_MenuItem");
            
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
            
            
        }
    }
}