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
    public class IndexModel : PageModel
    {
        private readonly TheGreenBowlContext _context;

        public IndexModel(TheGreenBowlContext context)
        {
            _context = context;
        }

        public IList<MenuViewModel> Menus { get; set; } = new List<MenuViewModel>();
        public bool IsUserLoggedIn { get; set; }
        public string ReturnUrl { get; set; }
        public List<BasketItemInfo> UserBasketItems { get; set; } = new List<BasketItemInfo>();

        public class BasketItemInfo
        {
            public int BasketItemID { get; set; }
            public int ItemID { get; set; }
            public int Quantity { get; set; }
        }

        public async Task OnGetAsync()
        {
            bool isAdmin = User.IsInRole("Admin");
            
            // Check if user is authenticated
            IsUserLoggedIn = User.Identity.IsAuthenticated;
            ReturnUrl = Url.Page("/Menu/Index");
    
            Menus = await _context.tblMenus
                .Include(m => m.Categories)
                .ThenInclude(mc => mc.Category)
                .Include(m => m.MenuItems)
                .ThenInclude(mi => mi.menuItem)
                .Select(menu => new MenuViewModel
                {
                    menuID = menu.menuID,
                    name = menu.name,
                    description = menu.description,
                    Categories = menu.Categories.Select(mc => mc.Category).ToList(),
                    MenuItems = menu.MenuItems.Select(mi => new MenuItemViewModel
                    {
                        itemID = mi.menuItem.itemID,
                        name = mi.menuItem.name,
                        description = mi.menuItem.description,
                        price = mi.menuItem.price,
                        ImageData = mi.menuItem.ImageData,
                        ImageDescription = mi.menuItem.ImageDescription
                    }).ToList(),
                    TotalItems = menu.MenuItems.Count
                })
                .ToListAsync();
    
            ViewData["IsAdmin"] = isAdmin;
            
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

            if (basket == null)
            {
                return new JsonResult(new { success = false, message = "Basket not found" });
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
        
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostAddToBasketAjaxAsync([FromBody] AddToBasketRequest request)
        {
            // Checking if the user is logged in
            if (!User.Identity.IsAuthenticated)
            {
                return new JsonResult(new { success = false, message = "Not logged in" });
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

    public class MenuViewModel
    {
        public int menuID { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public List<tblCategory> Categories { get; set; }
        public List<MenuItemViewModel> MenuItems { get; set; }
        public int TotalItems { get; set; }
    }

    public class MenuItemViewModel
    {
        public int itemID { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public decimal price { get; set; }
        public byte[] ImageData { get; set; }
        public string ImageDescription { get; set; }
    }
}
