using System.Collections.Generic;

namespace TheGreenBowl.Models
{
    public class tblMenuCategory
    {
        public int menuID { get; set; }
        public int categoryID { get; set; }

        // Navigation properties
        public tblMenu Menu { get; set; }
        public tblCategory Category { get; set; }
    }
}