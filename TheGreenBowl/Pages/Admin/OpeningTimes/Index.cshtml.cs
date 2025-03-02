using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TheGreenBowl.Data;
using TheGreenBowl.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace TheGreenBowl.Pages.Admin
{
    public class OpeningTimesModel : PageModel
    {
        private readonly TheGreenBowlContext _context;

        public OpeningTimesModel(TheGreenBowlContext context)
        {
            _context = context;
        }

        // Always initialize to an empty list so the view doesn’t crash if there are no records.
        public List<tblOpeningTimes> OpeningTimes { get; set; } = new List<tblOpeningTimes>();

        public async Task OnGetAsync()
        {
            OpeningTimes = await _context.tblOpeningTimes
                .OrderBy(ot => ot.DayOfWeek)
                .ToListAsync();
        }

        // Handler for delete – matches asp-page-handler="Delete"
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {

            if (id <= 0)
            {
                // Return the page with an error if the id is invalid.
                ModelState.AddModelError(string.Empty, "Invalid id");
                await OnGetAsync();
                return Page();
            }

            var item = await _context.tblOpeningTimes.FindAsync(id);
            if (item != null)
            {
                _context.tblOpeningTimes.Remove(item);
                await _context.SaveChangesAsync();
            }
            // Redirect so that the OnGetAsync runs and re-populates the list.
            return RedirectToPage();
        }
    }
}
