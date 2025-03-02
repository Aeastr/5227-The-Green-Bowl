using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TheGreenBowl.Data;
using TheGreenBowl.Models;
using Microsoft.EntityFrameworkCore;

namespace TheGreenBowl.Pages.Admin.OpeningTimes
{
    public class EditModel : PageModel
    {
        private readonly TheGreenBowlContext _context;
        public EditModel(TheGreenBowlContext context)
        {
            _context = context;
        }

        [BindProperty]
        public tblOpeningTimes Item { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                // If no id is provided, create a new entry with default values.
                Item = new tblOpeningTimes
                {
                    IsEnabled = true,
                    OpenTime = new TimeSpan(11, 0, 0),
                    CloseTime = new TimeSpan(22, 0, 0)
                };
                return Page();
            }

            Item = await _context.tblOpeningTimes.FindAsync(id);
            if (Item == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (Item.OpeningTimeId == 0)
            {
                // New item
                _context.tblOpeningTimes.Add(Item);
            }
            else
            {
                // Existing item, update record
                _context.Entry(Item).State = EntityState.Modified;
            }
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
        
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var item = await _context.tblOpeningTimes.FindAsync(id);
            if (item != null)
            {
                _context.tblOpeningTimes.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage("Index");
        }

    }
}