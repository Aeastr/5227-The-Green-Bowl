using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TheGreenBowl.Data;
using TheGreenBowl.Models;

namespace TheGreenBowl.Pages.Menu
{
    [Authorize(Roles = "Admin")]
    public class EditItemModel : PageModel
    {
        private readonly TheGreenBowlContext _context;

        public EditItemModel(TheGreenBowlContext context)
        {
            _context = context;
        }

        [BindProperty]
        public int MenuId { get; set; }

        [BindProperty]
        public tblMenuItem MenuItem { get; set; }

        public string MenuName { get; set; }

        public async Task<IActionResult> OnGetAsync(int menuId, int itemId)
        {
            MenuId = menuId;

            var menu = await _context.tblMenus
                .FirstOrDefaultAsync(m => m.menuID == menuId);

            if (menu == null)
            {
                return NotFound();
            }

            MenuName = menu.name;

            var menuItem = await _context.tblMenuItems
                .FirstOrDefaultAsync(mi => mi.itemID == itemId);

            if (menuItem == null)
            {
                return NotFound();
            }

            MenuItem = menuItem;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var menu = await _context.tblMenus
                    .FirstOrDefaultAsync(m => m.menuID == MenuId);
                
                if (menu != null)
                {
                    MenuName = menu.name;
                }
                
                return Page();
            }

            var existingItem = await _context.tblMenuItems
                .FirstOrDefaultAsync(mi => mi.itemID == MenuItem.itemID);

            if (existingItem == null)
            {
                return NotFound();
            }

            // Update the item properties
            existingItem.name = MenuItem.name;
            existingItem.description = MenuItem.description;
            existingItem.price = MenuItem.price;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await MenuItemExists(MenuItem.itemID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Details", new { id = MenuId });
        }

        private async Task<bool> MenuItemExists(int id)
        {
            return await _context.tblMenuItems.AnyAsync(e => e.itemID == id);
        }
    }
}
