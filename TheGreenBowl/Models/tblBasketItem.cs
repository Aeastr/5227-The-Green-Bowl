using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
namespace TheGreenBowl.Models;

public class tblBasketItem
{
    [Key]
    public int basketItemID { get; set; }

    public int basketID { get; set; }

    public int itemID { get; set; }

    [Required]
    public int quantity { get; set; } = 1;

    // Navigation properties
    [ForeignKey("basketID")]
    public virtual tblBasket basket { get; set; }

    [ForeignKey("itemID")]
    public virtual tblMenuItem menuItem { get; set; }
}