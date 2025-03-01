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
    public class RemoveItemModel : PageModel
    {
        private readonly TheGreenBowlContext _context;

        public RemoveItemModel(TheGreenBowlContext context)
        {
            _context = context;
        }

        [BindProperty]
        public int MenuId { get; set; }

        [BindProperty]
        public int ItemId { get; set; }

        public tblMenuItem MenuItem { get; set; }
        public string MenuName { get; set; }

        public async Task<IActionResult> OnGetAsync(int menuId, int itemId)
        {
            MenuId = menuId;
            ItemId = itemId;

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
            var menuMenuItem = await _context.tblMenu_MenuItems
                .FirstOrDefaultAsync(mm => mm.menuID == MenuId && mm.itemID == ItemId);

            if (menuMenuItem == null)
            {
                return NotFound();
            }

            _context.tblMenu_MenuItems.Remove(menuMenuItem);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Details", new { id = MenuId });
        }
    }
}