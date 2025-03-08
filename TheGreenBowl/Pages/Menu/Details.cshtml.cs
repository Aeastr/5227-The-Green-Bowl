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

namespace TheGreenBowl.Pages.Menu
{
    public class DetailsModel : PageModel
    {
        private readonly TheGreenBowl.Data.TheGreenBowlContext _context;

        public DetailsModel(TheGreenBowl.Data.TheGreenBowlContext context)
        {
            _context = context;
        }
        
        public bool IsUserLoggedIn { get; set; }
        public string ReturnUrl { get; set; }

        public tblMenu tblMenu { get; set; } = default!;
        
        public List<BasketItemInfo> UserBasketItems { get; set; } = new List<BasketItemInfo>();

        public class BasketItemInfo
        {
            public int BasketItemID { get; set; }
            public int ItemID { get; set; }
            public int Quantity { get; set; }
        }

        // Update the OnGetAsync method to load basket items
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Load the menu with its related menu items and categories
            var tblmenu = await _context.tblMenus
                .Include(m => m.Menu_MenuItems) // Include the relationship to MenuItems
                .ThenInclude(mm => mm.menuItem) // Include the actual menu item data
                .Include(m => m.Categories) // Include the relationship to Categories
                .ThenInclude(mc => mc.Category) // Include the actual category data
                .FirstOrDefaultAsync(m => m.menuID == id);

            if (tblmenu == null)
            {
                return NotFound();
            }
            else
            {
                tblMenu = tblmenu;
            }
    
            // Check if user is authenticated
            IsUserLoggedIn = User.Identity.IsAuthenticated;
            ReturnUrl = Url.Page("/Menu/Details", new { id = tblmenu.menuID });

            // If user is logged in, load their basket items
            if (IsUserLoggedIn)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var basket = await _context.tblBaskets
                    .Include(b => b.basketItems)
                    .FirstOrDefaultAsync(b => b.userID == userId);

                if (basket != null)
                {
                    UserBasketItems = basket.basketItems
                        .Select(bi => new BasketItemInfo
                        {
                            BasketItemID = bi.basketItemID,
                            ItemID = bi.itemID,
                            Quantity = bi.quantity
                        })
                        .ToList();
                }
            }

            return Page();
        }

        // Handler for updating quantities of items asynchronously
        public async Task<IActionResult> OnPostUpdateQuantityAsync(int itemId, int quantity, string operation, string returnUrl)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return new JsonResult(new { success = false, message = "Not authenticated" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var basket = await _context.tblBaskets
                .Include(b => b.basketItems)
                .FirstOrDefaultAsync(b => b.userID == userId);

            // check if user has a basket (they should as each user gets one on account creation
            if (basket == null)
            {
                return new JsonResult(new { success = false, message = "Basket not found" });
            }
            else
            {
                // logic for when they do not have a basket (maybe we create it?)
            }

            var basketItem = basket.basketItems.FirstOrDefault(bi => bi.itemID == itemId);
            if (basketItem == null)
            {
                return new JsonResult(new { success = false, message = "Item not in basket" });
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

            // Define an upper limit
            const int maxAllowedQuantity = 1000;
            if (basketItem.quantity > maxAllowedQuantity)
            {
                basketItem.quantity = maxAllowedQuantity;
            }

            bool removed = false;
            if (basketItem.quantity <= 0)
            {
                _context.tblBasketItems.Remove(basketItem);
                removed = true;
            }

            await _context.SaveChangesAsync();

            return new JsonResult(new { 
                success = true, 
                newQuantity = basketItem.quantity, 
                removed = removed
            });
        }
        
        [IgnoreAntiforgeryToken] // Since we're sending JSON, we need to ignore the antiforgery token
        // Handler for inital add to cart action, before showing the incremental UI using update handler
        public async Task<IActionResult> OnPostAddToBasketAjaxAsync([FromBody] AddToBasketRequest request)
        {
            // Checking if the user is logged in
            if (!User.Identity.IsAuthenticated)
            {
                return new JsonResult(new { success = false, message = "Not logged in" });
            }
            else
            {
                // we could direct them to the login/register page if not..
            }

            int itemId = request.ItemId;
    
            // Get the current user's ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    
            // Find the user's basket (or create one if it doesn't exist)
            var basket = await _context.tblBaskets
                .Include(b => b.basketItems)
                .FirstOrDefaultAsync(b => b.userID == userId);

            if (basket == null)
            {
                // Create a new basket for this user
                basket = new tblBasket
                {
                    userID = userId,
                    createdAt = DateTime.Now,
                    basketItems = new List<tblBasketItem>()
                };
                _context.tblBaskets.Add(basket);
                await _context.SaveChangesAsync();
            }

            // Check if the item is already in the basket
            var existingItem = basket.basketItems
                .FirstOrDefault(bi => bi.itemID == itemId);

            if (existingItem != null)
            {
                // In case our UI get's out of sync, add a case for when the incremental UI should be showing, but is not
                // Item already exists, increment quantity
                existingItem.quantity += 1;
            }
            else
            {
                // Add new item to basket
                var basketItem = new tblBasketItem
                {
                    basketID = basket.basketID,
                    itemID = itemId,
                    quantity = 1
                };
                basket.basketItems.Add(basketItem);
            }

            // Save changes
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }

        // Class to deserialize the JSON request
        public class AddToBasketRequest
        {
            public int ItemId { get; set; }
            public string ReturnUrl { get; set; }
        }
    }
}