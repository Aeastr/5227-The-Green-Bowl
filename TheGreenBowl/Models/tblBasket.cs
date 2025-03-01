using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace TheGreenBowl.Models;

public class tblBasket
{
    [Key]
    public int basketID { get; set; }

    [Required]
    public string userID { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime createdAt { get; set; } = DateTime.Now;

    // Navigation properties
    [ForeignKey("userID")]
    public virtual ApplicationUser user { get; set; }

    public virtual ICollection<tblBasketItem> basketItems { get; set; }
}