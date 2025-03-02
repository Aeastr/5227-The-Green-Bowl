using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGreenBowl.Models
{
    public class tblOrder
    {
        [Key]
        public int orderID { get; set; }

        [Required]
        public string userID { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime orderDate { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal totalAmount { get; set; }

        [NotMapped] // we do not need to store this in our database, we'll store the total amount tthough
        // the difference being that totalAmount will be the amount at the time of order, whereas calculated total will change if the menu items prices change!
        public decimal calculatedTotal => orderItems?.Sum(item => item.priceAtTime * item.quantity) ?? 0;

        [Required]
        public string orderStatus { get; set; } = "Pending";

        [Required]
        public string orderType { get; set; } // Delivery or Collection

        public string deliveryAddress { get; set; }
        public string postcode { get; set; }

        [Required]
        public string contactPhone { get; set; }
        
        [Required]
        [EmailAddress]
        public string contactEmail { get; set; }

        // Navigation properties
        [ForeignKey("userID")]
        public virtual ApplicationUser user { get; set; }

        public virtual ICollection<tblOrderItem> orderItems { get; set; }

        // Validation and utility methods
        public bool ValidateTotal()
        {
            return Math.Abs(totalAmount - calculatedTotal) < 0.01m; // Using small epsilon for decimal comparison
        }

        public void UpdateTotalFromItems()
        {
            totalAmount = calculatedTotal;
        }

        // Helper method to create an order from a basket
        public static tblOrder CreateFromBasket(tblBasket basket, string orderType, 
            string contactPhone, string contactEmail, string deliveryAddress = null, 
            string postcode = null)
        {
            var order = new tblOrder
            {
                userID = basket.userID,
                orderType = orderType,
                contactPhone = contactPhone,
                contactEmail = contactEmail,
                deliveryAddress = deliveryAddress,
                postcode = postcode,
                orderItems = basket.basketItems.Select(bi => new tblOrderItem
                {
                    itemID = bi.itemID,
                    quantity = bi.quantity,
                    priceAtTime = bi.menuItem.price
                }).ToList()
            };

            order.UpdateTotalFromItems();
            return order;
        }
    }
}
