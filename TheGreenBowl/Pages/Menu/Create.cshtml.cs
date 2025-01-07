using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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
            // Check if the form data is valid
            if (!ModelState.IsValid)
            {
                // Log validation errors for debugging
                foreach (var modelError in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Validation Error: {modelError.ErrorMessage}");
                }
                return Page();
            }

            try
            {
                // Add the menu item to the database context
                _context.tblMenus.Add(tblMenu);

                // Attempt to save changes to the database
                await _context.SaveChangesAsync();

                // Redirect to the Index page upon successful save
                return RedirectToPage("./Index");
            }
            catch (DbUpdateException dbEx)
            {
                // Log database-specific errors for debugging
                Console.WriteLine($"Database Update Error: {dbEx.InnerException?.Message ?? dbEx.Message}");

                // Add a user-friendly error message
                ModelState.AddModelError(string.Empty, "Unable to save changes. Please check your input and try again.");

                return Page();
            }
            catch (Exception ex)
            {
                // Log general exceptions for debugging
                Console.WriteLine($"General Error: {ex.Message}");

                // Add a user-friendly error message
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");

                return Page();
            }
        }
    }
}
