using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGreenBowl.Models
{
    public class MenuItem
    {
        [Key]
        public int ItemId { get; set; } // maps to item_id

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        public string Description { get; set; }
        
        /// specifies the SQL column type and precision for the `Price` property
        /// ensures that the Price is stored in the database with a precision of 10 digits in total
        /// and 2 digits after the decimal point. This is ideal for representing currency values accurately!!
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        // foreign key to Category
        public int? CategoryId { get; set; }

        // navigation property to Category
        public Category Category { get; set; }

        // navigation property to Menu_MenuItem junction table
        public ICollection<Menu_MenuItem> Menu_MenuItems { get; set; } // menus this item belongs to
    }
}