using System.Collections.Generic;

namespace TheGreenBowl.Models
{
    public class tblMenu_MenuItem
    {
        public int menuID { get; set; } // foreign key to tblMenu
        public int itemID { get; set; } // foreign key to tblMenuItem

        // navigation properties
        public tblMenu menu { get; set; } // reference to the menu
        public tblMenuItem menuItem { get; set; } // reference to the menu item
    }
}