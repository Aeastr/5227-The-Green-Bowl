using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TheGreenBowl.Models
{
    public class tblCategory
    {
        [Key]
        public int categoryID { get; set; }

        [Required]
        [StringLength(255)]
        public string name { get; set; }

        public string description { get; set; }

        public ICollection<tblMenuCategory> Menus { get; set; }
    }
}