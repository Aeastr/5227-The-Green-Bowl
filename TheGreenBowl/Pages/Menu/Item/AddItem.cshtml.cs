using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;  // For IFormFile
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

        // This property will catch uploaded files. Even though the form allows multiple,
        // you can loop through Request.Form.Files, which works with IFormFileCollection.
        // You can also bind to a property as shown below if you expect multiple files.
        [BindProperty]
        public List<IFormFile> ImageFiles { get; set; }

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
                // Validate the new item as needed
                if (!ModelState.IsValid)
                {
                    await LoadAvailableItems(menu);
                    return Page();
                }

                // Process the file upload(s)
                if (Request.Form.Files.Count > 0)
                {
                    foreach (var file in Request.Form.Files)
                    {
                        if (file.Length > 0)
                        {
                            using (MemoryStream ms = new MemoryStream())
                            {
                                await file.CopyToAsync(ms);
                                NewItem.ImageData = ms.ToArray();
                            }
                        }
                    }
                }

                _context.tblMenuItems.Add(NewItem);
                await _context.SaveChangesAsync();

                // Now add the new item to the menu
                menu.MenuItems.Add(new tblMenu_MenuItem
                {
                    menuID = MenuId,
                    itemID = NewItem.itemID
                });
            }
            else
            {
                if (!SelectedItemId.HasValue)
                {
                    ModelState.AddModelError("SelectedItemId", "Please select an item to add.");
                    await LoadAvailableItems(menu);
                    return Page();
                }

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
