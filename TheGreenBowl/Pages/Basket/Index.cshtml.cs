using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TheGreenBowl.Data;
using TheGreenBowl.Models;

namespace TheGreenBowl.Pages.Basket
{
    public class IndexModel : PageModel
    {
        private readonly TheGreenBowlContext _context;

        public IndexModel(TheGreenBowlContext context)
        {
            _context = context;
        }

        // The list of items in the basket (for display)
        public List<BasketItemViewModel> BasketItems { get; set; } = new List<BasketItemViewModel>();

        // The running total for the basket
        public decimal TotalPrice { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // If the user is not authenticated, redirect them to login.
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity", returnUrl = "/Basket/Index" });
            }

            // Get the current user's ID.
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Load the user's basket along with the basket items and their corresponding menu item details.
            var basket = await _context.tblBaskets
                .Include(b => b.basketItems)
                .ThenInclude(bi => bi.menuItem)
                .FirstOrDefaultAsync(b => b.userID == userId);

            if (basket != null)
            {
                // Create a view model list of basket items.
                BasketItems = basket.basketItems.Select(bi => new BasketItemViewModel
                {
                    BasketItemID = bi.basketItemID,
                    ItemID = bi.itemID,
                    MenuItemName = bi.menuItem.name,
                    MenuItemDescription = bi.menuItem.description,
                    MenuItemPrice = bi.menuItem.price,
                    ImageData = bi.menuItem.ImageData,
                    ImageDescription = bi.menuItem.ImageDescription,
                    Quantity = bi.quantity,
                    Subtotal = bi.quantity * bi.menuItem.price
                }).ToList();

                // Calculate the total price for the basket.
                TotalPrice = BasketItems.Sum(item => item.Subtotal);
            }
            else
            {
                // If no basket exists, show an empty list
                BasketItems = new List<BasketItemViewModel>();
                TotalPrice = 0;
            }

            return Page();
        }

        // A handler to update the quantity of an item.
        // If the new quantity is 0 (or less), the item is removed.
        public async Task<IActionResult> OnPostUpdateQuantityAsync(int basketItemId, int quantity, string operation)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return new JsonResult(new { success = false, message = "Not authenticated" });
            }

            var basketItem = await _context.tblBasketItems.FindAsync(basketItemId);
            if (basketItem == null)
            {
                return new JsonResult(new { success = false, message = "Basket item not found" });
            }

            // If the operation is null (for example when pressing Enter), default to "update"
            if (string.IsNullOrWhiteSpace(operation))
            {
                operation = "update";
            }

            // Process the update based on operation
            switch (operation.ToLower())
            {
                case "increment":
                    basketItem.quantity += 1;
                    break;
                case "decrement":
                    basketItem.quantity -= 1;
                    break;
                case "update":
                    basketItem.quantity = quantity;
                    break;
                default:
                    break;
            }

            // Define an upper limit (e.g., 1000)
            const int maxAllowedQuantity = 1000;
            if (basketItem.quantity > maxAllowedQuantity)
            {
                // clamp the quantity.
                basketItem.quantity = maxAllowedQuantity;
            }

            // Logic for if quantity is 0 or lower (we remove it from the cart)
            bool removed = false;
            if (basketItem.quantity <= 0)
            {
                _context.tblBasketItems.Remove(basketItem);
                removed = true;
                await _context.SaveChangesAsync();
            }
            else
            {
                await _context.SaveChangesAsync();
            }

            // Recalculate the basket total for the user.
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var basket = await _context.tblBaskets
                .Include(b => b.basketItems)
                .ThenInclude(bi => bi.menuItem)
                .FirstOrDefaultAsync(b => b.userID == userId);
            decimal newTotal = basket?.basketItems.Sum(bi => bi.quantity * bi.menuItem.price) ?? 0;

            
            if (removed)
            {
                // If we did remove it, return the result and new basket total
                return new JsonResult(new { success = true, removed = true, newTotal });
            }
            else
            {
                // If not, retrieve the unit price to compute the new subtotal for the item.
                var unitPrice = await _context.tblMenuItems
                    .Where(mi => mi.itemID == basketItem.itemID)
                    .Select(mi => mi.price)
                    .FirstOrDefaultAsync();
                var newSubtotal = basketItem.quantity * unitPrice;
                return new JsonResult(new { 
                    success = true, 
                    newQuantity = basketItem.quantity, 
                    newSubtotal, 
                    removed, 
                    newTotal 
                });
            }
        }

    }

    // View model for each basket item.
    public class BasketItemViewModel
    {
        public int BasketItemID { get; set; }
        public int ItemID { get; set; }
        public string MenuItemName { get; set; }
        public string MenuItemDescription { get; set; }
        public decimal MenuItemPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
        public byte[] ImageData { get; set; }
        public string ImageDescription { get; set; }
    }
}