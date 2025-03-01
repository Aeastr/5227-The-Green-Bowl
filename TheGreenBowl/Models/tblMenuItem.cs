using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGreenBowl.Models
{
    public class tblMenuItem
    {
        [Key]
        public int itemID { get; set; }

        [Required]
        [StringLength(255)]
        public string name { get; set; }

        public string description { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal price { get; set; }
        
        public byte[] ImageData { get; set; }
        
        public string ImageDescription { get; set; }
    }
}