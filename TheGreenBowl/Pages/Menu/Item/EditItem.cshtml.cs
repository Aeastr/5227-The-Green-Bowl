using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TheGreenBowl.Data;
using TheGreenBowl.Models;

namespace TheGreenBowl.Pages.Menu.Item
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
        public tblMenuItem MenuItem { get; set; }

        [BindProperty]
        public string ReturnUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(int itemId, string returnUrl = null)
        {
            MenuItem = await _context.tblMenuItems.FirstOrDefaultAsync(mi => mi.itemID == itemId);
            if (MenuItem == null)
                return NotFound();

            // Set return URL (either from parameter or default to JavaScript history back)
            ReturnUrl = returnUrl ?? "javascript:history.back()";

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var existingItem = await _context.tblMenuItems.FirstOrDefaultAsync(mi => mi.itemID == MenuItem.itemID);
            if (existingItem == null)
                return NotFound();

            // Update properties
            existingItem.name = MenuItem.name;
            existingItem.description = MenuItem.description;
            existingItem.price = MenuItem.price;
            existingItem.ImageDescription = MenuItem.ImageDescription;

            // Process new image upload if any
            if (Request.Form.Files.Count > 0)
            {
                foreach (var file in Request.Form.Files)
                {
                    if (file.Length > 0)
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            await file.CopyToAsync(ms);
                            existingItem.ImageData = ms.ToArray();
                        }
                    }
                }
            }

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

            // Return to the provided return URL or use JavaScript history back
            return Redirect(ReturnUrl);
        }

        private async Task<bool> MenuItemExists(int id)
        {
            return await _context.tblMenuItems.AnyAsync(e => e.itemID == id);
        }
    }
}
