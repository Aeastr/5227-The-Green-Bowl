using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGreenBowl.Models
{
    public class tblOrderItem
    {
        [Key]
        public int orderItemID { get; set; }

        public int orderID { get; set; }

        public int itemID { get; set; }

        [Required]
        public int quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal priceAtTime { get; set; } // Store price at time of order

        // Navigation properties
        [ForeignKey("orderID")]
        public virtual tblOrder order { get; set; }

        [ForeignKey("itemID")]
        public virtual tblMenuItem menuItem { get; set; }
    }
}