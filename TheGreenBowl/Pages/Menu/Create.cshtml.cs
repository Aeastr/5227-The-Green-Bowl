using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TheGreenBowl.Data;
using TheGreenBowl.Models;

namespace TheGreenBowl.Pages.Menu
{
    public class CreateModel : PageModel
    {
        private readonly TheGreenBowl.Data.TheGreenBowlContext _context;

        public CreateModel(TheGreenBowl.Data.TheGreenBowlContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public tblMenu tblMenu { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Menus.Add(tblMenu);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
