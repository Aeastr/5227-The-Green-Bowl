using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGreenBowl.Models
{
    public class tblMenuCategory
    {
        [Key]
        [Column(Order = 1)]
        public int menuID { get; set; }
        
        [Key]
        [Column(Order = 2)]
        public int categoryID { get; set; }

        // Navigation properties
        [ForeignKey("menuID")]
        public virtual tblMenu Menu { get; set; }
        
        [ForeignKey("categoryID")]
        public virtual tblCategory Category { get; set; }
    }
}