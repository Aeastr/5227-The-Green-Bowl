using System.ComponentModel.DataAnnotations.Schema;

namespace TheGreenBowl.Models
{
    public class tblMenu_MenuItem
    {
        // composite key will be configured in DbContext

        public int MenuId { get; set; } // foreign key to Menu
        public int ItemId { get; set; } // foreign key to MenuItem

        // navigation properties
        public tblMenu TblMenu { get; set; }
        public tblMenuItem TblMenuItem { get; set; }
    }
}