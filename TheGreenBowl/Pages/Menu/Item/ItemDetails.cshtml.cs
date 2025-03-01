using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TheGreenBowl.Data;
using TheGreenBowl.Models;

namespace TheGreenBowl.Pages.Menu.Item
{
    public class ItemDetailsModel : PageModel
    {
        private readonly TheGreenBowl.Data.TheGreenBowlContext _context;

        public ItemDetailsModel(TheGreenBowl.Data.TheGreenBowlContext context)
        {
            _context = context;
        }

        public tblMenuItem MenuItem { get; set; } = default!;
        
        [BindProperty(SupportsGet = true)]
        public int MenuId { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id, int? menuId)
        {
            if (id == null)
            {
                return NotFound();
            }

            if (menuId.HasValue)
            {
                MenuId = menuId.Value;
            }

            // Load the menu item
            var menuItem = await _context.tblMenuItems
                .FirstOrDefaultAsync(m => m.itemID == id);

            if (menuItem == null)
            {
                return NotFound();
            }
            else
            {
                MenuItem = menuItem;
            }
            return Page();
        }
    }
}