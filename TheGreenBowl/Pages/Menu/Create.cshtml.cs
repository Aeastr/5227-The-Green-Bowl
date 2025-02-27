using System;
using System.Collections.Generic;
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
    public class CreateModel : PageModel
    {
        private readonly TheGreenBowl.Data.TheGreenBowlContext _context;

        public CreateModel(TheGreenBowl.Data.TheGreenBowlContext context)
        {
            _context = context;
        }

        [BindProperty]
        public tblMenu tblMenu { get; set; } = default!;
        
        [BindProperty]
        public List<int> SelectedCategoryIds { get; set; } = new List<int>();
        
        public List<SelectListItem> AvailableCategories { get; set; } = new List<SelectListItem>();

        public async Task<IActionResult> OnGetAsync()
        {
            // Get all available categories
            AvailableCategories = await _context.tblCategories
                .Select(c => new SelectListItem
                {
                    Value = c.categoryID.ToString(),
                    Text = c.name
                })
                .ToListAsync();
                
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Check if the form data is valid
            if (!ModelState.IsValid)
            {
                // Reload categories for the form if validation fails
                AvailableCategories = await _context.tblCategories
                    .Select(c => new SelectListItem
                    {
                        Value = c.categoryID.ToString(),
                        Text = c.name
                    })
                    .ToListAsync();
                    
                return Page();
            }

            // Start a transaction to ensure all changes are applied together
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Initialize the Categories collection if it's null
                if (tblMenu.Categories == null)
                {
                    tblMenu.Categories = new List<tblMenuCategory>();
                }
                
                // Add the menu to the database
                _context.tblMenus.Add(tblMenu);
                await _context.SaveChangesAsync();
                
                // Now that we have the menu ID, add the category relationships
                foreach (var categoryId in SelectedCategoryIds)
                {
                    tblMenu.Categories.Add(new tblMenuCategory
                    {
                        menuID = tblMenu.menuID,
                        categoryID = categoryId
                    });
                }
                
                // Save the category relationships
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                return RedirectToPage("./Index");
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();
                
                // Log database-specific errors for debugging
                Console.WriteLine($"Database Update Error: {dbEx.InnerException?.Message ?? dbEx.Message}");

                // Add a user-friendly error message
                ModelState.AddModelError(string.Empty, "Unable to save changes. Please check your input and try again.");
                
                // Reload categories
                AvailableCategories = await _context.tblCategories
                    .Select(c => new SelectListItem
                    {
                        Value = c.categoryID.ToString(),
                        Text = c.name
                    })
                    .ToListAsync();
                    
                return Page();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                
                // Log general exceptions for debugging
                Console.WriteLine($"General Error: {ex.Message}");

                // Add a user-friendly error message
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
                
                // Reload categories
                AvailableCategories = await _context.tblCategories
                    .Select(c => new SelectListItem
                    {
                        Value = c.categoryID.ToString(),
                        Text = c.name
                    })
                    .ToListAsync();
                    
                return Page();
            }
        }
    }
}
