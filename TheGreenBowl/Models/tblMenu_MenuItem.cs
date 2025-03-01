using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGreenBowl.Models
{
    public class tblMenu_MenuItem
    {
        [Key]
        [Column(Order = 1)]
        public int menuID { get; set; }
        
        [Key]
        [Column(Order = 2)]
        public int itemID { get; set; }

        // Navigation properties
        [ForeignKey("menuID")]
        public virtual tblMenu menu { get; set; }
        
        [ForeignKey("itemID")]
        public virtual tblMenuItem menuItem { get; set; }
    }
}