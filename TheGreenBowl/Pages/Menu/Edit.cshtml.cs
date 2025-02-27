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
    public class EditModel : PageModel
    {
        private readonly TheGreenBowl.Data.TheGreenBowlContext _context;

        public EditModel(TheGreenBowl.Data.TheGreenBowlContext context)
        {
            _context = context;
        }

        [BindProperty]
        public tblMenu tblMenu { get; set; } = default!;
        
        [BindProperty]
        public List<int> SelectedCategoryIds { get; set; } = new List<int>();
        
        public List<SelectListItem> AvailableCategories { get; set; } = new List<SelectListItem>();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblmenu = await _context.tblMenus
                .Include(m => m.Categories)
                .ThenInclude(mc => mc.Category)
                .FirstOrDefaultAsync(m => m.menuID == id);
                
            if (tblmenu == null)
            {
                return NotFound();
            }
            
            tblMenu = tblmenu;
            
            // Get all available categories
            AvailableCategories = await _context.tblCategories
                .Select(c => new SelectListItem
                {
                    Value = c.categoryID.ToString(),
                    Text = c.name
                })
                .ToListAsync();
                
            // Set the currently selected categories
            SelectedCategoryIds = tblMenu.Categories
                .Select(mc => mc.categoryID)
                .ToList();
                
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
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
                // Get the existing menu with its categories
                var existingMenu = await _context.tblMenus
                    .Include(m => m.Categories)
                    .FirstOrDefaultAsync(m => m.menuID == tblMenu.menuID);
                
                if (existingMenu == null)
                {
                    return NotFound();
                }
                
                // Update basic menu properties
                existingMenu.name = tblMenu.name;
                existingMenu.description = tblMenu.description;
                
                // Remove categories that are no longer selected
                var categoriesToRemove = existingMenu.Categories
                    .Where(mc => !SelectedCategoryIds.Contains(mc.categoryID))
                    .ToList();
                    
                foreach (var category in categoriesToRemove)
                {
                    existingMenu.Categories.Remove(category);
                }
                
                // Add newly selected categories
                var existingCategoryIds = existingMenu.Categories
                    .Select(mc => mc.categoryID)
                    .ToList();
                    
                var categoriesToAdd = SelectedCategoryIds
                    .Where(id => !existingCategoryIds.Contains(id))
                    .ToList();
                    
                foreach (var categoryId in categoriesToAdd)
                {
                    existingMenu.Categories.Add(new tblMenuCategory
                    {
                        menuID = existingMenu.menuID,
                        categoryID = categoryId
                    });
                }
                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }

            return RedirectToPage("./Index");
        }

        private bool tblMenuExists(int id)
        {
            return _context.tblMenus.Any(e => e.menuID == id);
        }
    }
}
