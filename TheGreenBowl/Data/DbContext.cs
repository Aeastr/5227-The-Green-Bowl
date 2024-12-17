using Microsoft.EntityFrameworkCore;
using TheGreenBowl.Models;

namespace TheGreenBowl.Data
{
    public class TheGreenBowlContext : DbContext
    {
        public TheGreenBowlContext(DbContextOptions<TheGreenBowlContext> options)
            : base(options)
        {
        }

        // DbSet for each table in the database
        public DbSet<tblMenu> Menus { get; set; }
        public DbSet<tblMenuItem> MenuItems { get; set; }
        public DbSet<tblCategory> Categories { get; set; }
        public DbSet<tblMenu_MenuItem> Menu_MenuItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Map each entity to its corresponding table
            modelBuilder.Entity<tblMenu>().ToTable("Menu");
            modelBuilder.Entity<tblMenuItem>().ToTable("MenuItem");
            modelBuilder.Entity<tblCategory>().ToTable("Category");
            modelBuilder.Entity<tblMenu_MenuItem>().ToTable("Menu_MenuItem");

            // Configure composite key for Menu_MenuItem
            modelBuilder.Entity<tblMenu_MenuItem>()
                .HasKey(mmi => new { mmi.MenuId, mmi.ItemId });

            // Configure many-to-many relationship: Menu ↔ MenuItem (via Menu_MenuItem)
            modelBuilder.Entity<tblMenu_MenuItem>()
                .HasOne(mmi => mmi.TblMenu)
                .WithMany(menu => menu.MenuItems)
                .HasForeignKey(mmi => mmi.MenuId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<tblMenu_MenuItem>()
                .HasOne(mmi => mmi.TblMenuItem)
                .WithMany(menuItem => menuItem.Menu_MenuItems)
                .HasForeignKey(mmi => mmi.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure one-to-many relationship: MenuItem → Category
            modelBuilder.Entity<tblMenuItem>()
                .HasOne(menuItem => menuItem.TblCategory)
                .WithMany(category => category.MenuItems)
                .HasForeignKey(menuItem => menuItem.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // // Optional: Other tables like Order and OrderItem if implemented in C#
            // modelBuilder.Entity<Order>().ToTable("Order");
            // modelBuilder.Entity<OrderItem>().ToTable("OrderItem");
            //
            // // Composite key for OrderItem
            // modelBuilder.Entity<OrderItem>()
            //     .HasKey(oi => new { oi.OrderId, oi.ItemId });
            //
            // // Configure one-to-many relationship: OrderItem → Order
            // modelBuilder.Entity<OrderItem>()
            //     .HasOne(oi => oi.Order)
            //     .WithMany(order => order.OrderItems)
            //     .HasForeignKey(oi => oi.OrderId)
            //     .OnDelete(DeleteBehavior.Cascade);
            //
            // // Configure one-to-many relationship: OrderItem → MenuItem
            // modelBuilder.Entity<OrderItem>()
            //     .HasOne(oi => oi.MenuItem)
            //     .WithMany(menuItem => menuItem.OrderItems)
            //     .HasForeignKey(oi => oi.ItemId)
            //     .OnDelete(DeleteBehavior.Cascade);
        }
    }
}