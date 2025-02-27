using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TheGreenBowl.Data;
using TheGreenBowl.Models;

namespace TheGreenBowl.Pages.Menu
{
    [Authorize(Roles = "Admin")]
    public class AddItemModel : PageModel
    {
        private readonly TheGreenBowlContext _context;

        public AddItemModel(TheGreenBowlContext context)
        {
            _context = context;
        }

        [BindProperty]
        public int MenuId { get; set; }

        public string MenuName { get; set; }

        [BindProperty]
        public int? SelectedItemId { get; set; }

        [BindProperty]
        public bool CreateNewItem { get; set; }

        [BindProperty]
        public tblMenuItem NewItem { get; set; } = new tblMenuItem();

        public List<SelectListItem> AvailableItems { get; set; } = new List<SelectListItem>();

        public async Task<IActionResult> OnGetAsync(int menuId)
        {
            MenuId = menuId;

            var menu = await _context.tblMenus
                .Include(m => m.MenuItems)
                .FirstOrDefaultAsync(m => m.menuID == menuId);

            if (menu == null)
            {
                return NotFound();
            }

            MenuName = menu.name;

            // Get all menu items that are not already in this menu
            var existingItemIds = menu.MenuItems.Select(mi => mi.itemID).ToList();
            
            AvailableItems = await _context.tblMenuItems
                .Where(item => !existingItemIds.Contains(item.itemID))
                .Select(item => new SelectListItem
                {
                    Value = item.itemID.ToString(),
                    Text = $"{item.name} (${item.price:F2})"
                })
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var menu = await _context.tblMenus
                .Include(m => m.MenuItems)
                .FirstOrDefaultAsync(m => m.menuID == MenuId);

            if (menu == null)
            {
                return NotFound();
            }

            if (CreateNewItem)
            {
                // Validate the new item
                if (!ModelState.IsValid)
                {
                    // Reload the available items for the dropdown
                    await LoadAvailableItems(menu);
                    return Page();
                }

                // Add the new item to the database
                _context.tblMenuItems.Add(NewItem);
                await _context.SaveChangesAsync();

                // Add the new item to the menu
                menu.MenuItems.Add(new tblMenu_MenuItem
                {
                    menuID = MenuId,
                    itemID = NewItem.itemID
                });
            }
            else
            {
                // Validate the selected item
                if (!SelectedItemId.HasValue)
                {
                    ModelState.AddModelError("SelectedItemId", "Please select an item to add.");
                    await LoadAvailableItems(menu);
                    return Page();
                }

                // Add the selected item to the menu
                menu.MenuItems.Add(new tblMenu_MenuItem
                {
                    menuID = MenuId,
                    itemID = SelectedItemId.Value
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToPage("./Details", new { id = MenuId });
        }

        private async Task LoadAvailableItems(tblMenu menu)
        {
            var existingItemIds = menu.MenuItems.Select(mi => mi.itemID).ToList();
            
            AvailableItems = await _context.tblMenuItems
                .Where(item => !existingItemIds.Contains(item.itemID))
                .Select(item => new SelectListItem
                {
                    Value = item.itemID.ToString(),
                    Text = $"{item.name} (${item.price:F2})"
                })
                .ToListAsync();

            MenuName = menu.name;
        }
    }
}
