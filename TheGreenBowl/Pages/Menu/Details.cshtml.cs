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
        private readonly TheGreenBowlContext _context;

        public DetailsModel(TheGreenBowlContext context)
        {
            _context = context;
        }
        
        // Flag to indicate if the user is logged in.
        public bool IsUserLoggedIn { get; set; }
        // Return URL for redirection after actions.
        public string ReturnUrl { get; set; }
        // The detailed menu being displayed.
        public tblMenu tblMenu { get; set; } = default!;
        // A dictionary of item IDs and their quantities in the user's basket.
        public Dictionary<int, int> BasketQuantities { get; set; } = new Dictionary<int, int>();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Load the menu with its related items and categories.
            var tblmenu = await _context.tblMenus
                .Include(m => m.MenuItems)        // Junction table for menu and menu items.
                .ThenInclude(mm => mm.menuItem)     // The actual menu item data.
                .Include(m => m.Categories)         // Junction table for menu and categories.
                .ThenInclude(mc => mc.Category)     // The actual category data.
                .FirstOrDefaultAsync(m => m.menuID == id);

            if (tblmenu == null)
                return NotFound();
            else
                tblMenu = tblmenu;
            
            // Determine if the user is logged in.
            bool isLoggedIn = User.Identity.IsAuthenticated;
            string returnUrl = Url.Page("/Menu/Details", new { id = tblmenu.menuID });
            IsUserLoggedIn = isLoggedIn;
            ReturnUrl = returnUrl;

            // If the user is logged in, load their basket (if it exists) and build the dictionary.
            if (isLoggedIn)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var basket = await _context.tblBaskets
                    .Include(b => b.basketItems)
                    .FirstOrDefaultAsync(b => b.userID == userId);

                if (basket != null)
                {
                    BasketQuantities = basket.basketItems
                        .ToDictionary(bi => bi.itemID, bi => bi.quantity);
                }
            }

            return Page();
        }

        // Handler for the original "Add to Basket" when item not already in basket.
        public async Task<IActionResult> OnPostAddToBasketAsync(int itemId, string returnUrl = null)
        {
            // Redirect to login if not authenticated.
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login",
                            new { area = "Identity", returnUrl = returnUrl ?? Url.Page("/Menu/Details", new { id = 0 }) });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Try to load the user's basket.
            var basket = await _context.tblBaskets
                .Include(b => b.basketItems)
                .FirstOrDefaultAsync(b => b.userID == userId);

            if (basket == null)
            {
                // If no basket exists, create one.
                basket = new tblBasket
                {
                    userID = userId,
                    createdAt = DateTime.Now,
                    basketItems = new List<tblBasketItem>()
                };
                _context.tblBaskets.Add(basket);
                await _context.SaveChangesAsync();
            }

            // Add a new basket item.
            var basketItem = new tblBasketItem
            {
                basketID = basket.basketID,
                itemID = itemId,
                quantity = 1
            };
            basket.basketItems.Add(basketItem);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Item added to basket!";
            return Redirect(returnUrl ?? Url.Page("/Menu/Details"));
        }

        // Handler to increment an item’s quantity in the basket.
        public async Task<IActionResult> OnPostIncrementAsync(int itemId, string returnUrl = null)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login",
                    new { area = "Identity", returnUrl = returnUrl });
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var basket = await _context.tblBaskets
                .Include(b => b.basketItems)
                .FirstOrDefaultAsync(b => b.userID == userId);
            if (basket == null)
            {
                // No basket? Create one just as in OnPostAddToBasket.
                basket = new tblBasket
                {
                    userID = userId,
                    createdAt = DateTime.Now,
                    basketItems = new List<tblBasketItem>()
                };
                _context.tblBaskets.Add(basket);
                await _context.SaveChangesAsync();
            }
            var existingItem = basket.basketItems.FirstOrDefault(bi => bi.itemID == itemId);
            if (existingItem != null)
            {
                existingItem.quantity++;
            }
            else
            {
                // If item not already present, simply add it.
                basket.basketItems.Add(new tblBasketItem
                {
                    basketID = basket.basketID,
                    itemID = itemId,
                    quantity = 1
                });
            }
            await _context.SaveChangesAsync();
            return Redirect(returnUrl ?? Url.Page("/Menu/Details"));
        }

        // Handler to decrement an item's quantity (and remove if quantity reaches zero).
        public async Task<IActionResult> OnPostDecrementAsync(int itemId, string returnUrl = null)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login",
                    new { area = "Identity", returnUrl = returnUrl });
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var basket = await _context.tblBaskets
                .Include(b => b.basketItems)
                .FirstOrDefaultAsync(b => b.userID == userId);
            if (basket == null)
            {
                return Redirect(returnUrl ?? Url.Page("/Menu/Details"));
            }
            var existingItem = basket.basketItems.FirstOrDefault(bi => bi.itemID == itemId);
            if (existingItem != null)
            {
                existingItem.quantity--;
                if (existingItem.quantity <= 0)
                {
                    _context.tblBasketItems.Remove(existingItem);
                }
                await _context.SaveChangesAsync();
            }
            return Redirect(returnUrl ?? Url.Page("/Menu/Details"));
        }
    }
}
