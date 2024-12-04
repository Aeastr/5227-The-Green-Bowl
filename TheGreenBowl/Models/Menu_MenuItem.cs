using System.ComponentModel.DataAnnotations.Schema;

namespace TheGreenBowl.Models
{
    public class Menu_MenuItem
    {
        // composite key will be configured in DbContext

        public int MenuId { get; set; } // foreign key to Menu
        public int ItemId { get; set; } // foreign key to MenuItem

        // navigation properties
        public Menu Menu { get; set; }
        public MenuItem MenuItem { get; set; }
    }
}