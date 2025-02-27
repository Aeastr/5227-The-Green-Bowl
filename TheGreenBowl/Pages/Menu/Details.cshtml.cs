using System;
using System.Collections.Generic;
using System.Linq;
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

        public tblMenu tblMenu { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Load the menu with its related menu items and categories
            var tblmenu = await _context.tblMenus
                .Include(m => m.MenuItems) // Include the relationship to MenuItems
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
            return Page();
        }
    }
}