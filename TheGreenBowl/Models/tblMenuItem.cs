using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGreenBowl.Models
{
    public class tblMenuItem
    {
        [Key]
        [Display(Name = "ID")]
        public int itemID { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Name")]
        public string name { get; set; }
        
        [Display(Name = "Details")]
        public string description { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Price")]
        public decimal price { get; set; }
        
        [Display(Name = "Image")]
        public byte[] ImageData { get; set; }
        
        public string ImageDescription { get; set; }
    }
}