using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TheGreenBowl.Data;
using TheGreenBowl.Models;

namespace TheGreenBowl.Pages.Checkout
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly TheGreenBowlContext _context;

        public IndexModel(TheGreenBowlContext context)
        {
            _context = context;
        }

        [BindProperty]
        public CheckoutViewModel CheckoutInfo { get; set; }

        public List<BasketItemViewModel> BasketItems { get; set; } = new List<BasketItemViewModel>();
        public decimal TotalPrice { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Get user's basket with items
            var basket = await _context.tblBaskets
                .Include(b => b.basketItems)
                .ThenInclude(bi => bi.menuItem)
                .FirstOrDefaultAsync(b => b.userID == userId);

            if (basket == null || !basket.basketItems.Any())
            {
                return RedirectToPage("/Basket/Index");
            }

            // Populate basket items for display
            foreach (var item in basket.basketItems)
            {
                var basketItem = new BasketItemViewModel
                {
                    BasketItemID = item.basketItemID,
                    MenuItemName = item.menuItem.name,
                    MenuItemDescription = item.menuItem.description,
                    MenuItemPrice = item.menuItem.price,
                    Quantity = item.quantity,
                    Subtotal = item.menuItem.price * item.quantity,
                    ImageData = item.menuItem.ImageData,
                    ImageDescription = item.menuItem.ImageDescription
                };

                BasketItems.Add(basketItem);
                TotalPrice += basketItem.Subtotal;
            }

            // Pre-populate checkout info with user details if available
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                CheckoutInfo = new CheckoutViewModel
                {
                    ContactEmail = user.Email,
                    ContactPhone = user.PhoneNumber ?? ""
                };
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync(); // Reload the page data
                return Page();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Get user's basket with items
            var basket = await _context.tblBaskets
                .Include(b => b.basketItems)
                .ThenInclude(bi => bi.menuItem)
                .FirstOrDefaultAsync(b => b.userID == userId);

            if (basket == null || !basket.basketItems.Any())
            {
                return RedirectToPage("/Basket/Index");
            }

            // Create new order from basket
            var order = new tblOrder
            {
                userID = userId,
                orderType = CheckoutInfo.OrderType,
                contactPhone = CheckoutInfo.ContactPhone,
                contactEmail = CheckoutInfo.ContactEmail,
                deliveryAddress = CheckoutInfo.OrderType == "Delivery" ? CheckoutInfo.DeliveryAddress : null,
                postcode = CheckoutInfo.OrderType == "Delivery" ? CheckoutInfo.Postcode : null,
                orderItems = new List<tblOrderItem>()
            };

            // Add order items
            foreach (var basketItem in basket.basketItems)
            {
                var orderItem = new tblOrderItem
                {
                    itemID = basketItem.itemID,
                    quantity = basketItem.quantity,
                    priceAtTime = basketItem.menuItem.price
                };
                
                order.orderItems.Add(orderItem);
            }

            // Calculate and set total
            order.UpdateTotalFromItems();

            // Save order to database
            _context.Orders.Add(order);
            
            // Remove basket
            _context.tblBaskets.Remove(basket);
            
            await _context.SaveChangesAsync();

            // Redirect to order confirmation
            return RedirectToPage("/Checkout/Confirmation", new { orderId = order.orderID });
        }
    }

    public class CheckoutViewModel
    {
        [Required]
        [Display(Name = "Order Type")]
        public string OrderType { get; set; } = "Collection"; // Collection or Delivery

        [Required]
        [Display(Name = "Contact Phone")]
        [RegularExpression(@"^\+?[1-9]\d{6,14}$", ErrorMessage = "Please enter a valid phone number (e.g., +1234567890).")]
        public string ContactPhone { get; set; }

        [Required]
        [Display(Name = "Contact Email")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string ContactEmail { get; set; }

        [Display(Name = "Delivery Address")]
        [RequiredIf("OrderType", "Delivery", ErrorMessage = "Delivery Address is required for delivery orders.")]
        [StringLength(255, ErrorMessage = "Delivery Address cannot exceed 255 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s,.'-]{3,}$", ErrorMessage = "Please enter a valid delivery address.")]
        public string DeliveryAddress { get; set; }


        [Display(Name = "Postcode")]
        [RequiredIf("OrderType", "Delivery", ErrorMessage = "Postcode is required for delivery orders.")]
        [RegularExpression(@"^[A-Za-z0-9 ]{3,10}$", ErrorMessage = "Please enter a valid postcode.")]
        public string Postcode { get; set; }
    }


    public class BasketItemViewModel
    {
        public int BasketItemID { get; set; }
        public string MenuItemName { get; set; }
        public string MenuItemDescription { get; set; }
        public decimal MenuItemPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
        public byte[] ImageData { get; set; }
        public string ImageDescription { get; set; }
    }

    // Custom validation attribute for conditional required fields
    public class RequiredIfAttribute : ValidationAttribute
    {
        private readonly string _dependentProperty;
        private readonly object _targetValue;

        public RequiredIfAttribute(string dependentProperty, object targetValue)
        {
            _dependentProperty = dependentProperty;
            _targetValue = targetValue;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var dependentPropertyValue = validationContext.ObjectType.GetProperty(_dependentProperty)
                .GetValue(validationContext.ObjectInstance, null);

            if (dependentPropertyValue?.ToString() == _targetValue.ToString())
            {
                if (value == null || (value is string stringValue && string.IsNullOrWhiteSpace(stringValue)))
                {
                    return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} is required.");
                }
            }

            return ValidationResult.Success;
        }
    }
}
