using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TheGreenBowl.Models
{
    public class tblMenu
    {
        [Key]
        public int menuID { get; set; }

        [Required]
        [StringLength(255)]
        public string name { get; set; }

        public string description { get; set; }

        // Navigation property for Menu-Category relationship
        public ICollection<tblMenuCategory> Categories { get; set; }

        // Navigation property for Menu-Item relationship
        public ICollection<tblMenu_MenuItem> Menu_MenuItems { get; set; }
    }
}