using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TheGreenBowl.Models
{
    public class Menu
    {
        [Key]
        public int MenuId { get; set; } // maps to menu_id

        [Required]
        [StringLength(255)]
        public string Name { get; set; } // name of the menu

        public string Description { get; set; } // optional description

        // navigation property
        public ICollection<Menu_MenuItem> MenuItems { get; set; } // items in this menu
    }
}