using System;
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
    public class ConfirmationModel : PageModel
    {
        private readonly TheGreenBowlContext _context;

        public ConfirmationModel(TheGreenBowlContext context)
        {
            _context = context;
        }

        public tblOrder Order { get; set; }

        public async Task<IActionResult> OnGetAsync(int orderId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            Order = await _context.Orders
                .Include(o => o.orderItems)
                .ThenInclude(oi => oi.menuItem)
                .FirstOrDefaultAsync(o => o.orderID == orderId && o.userID == userId);

            if (Order == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}