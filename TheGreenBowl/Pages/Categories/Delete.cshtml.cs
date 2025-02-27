using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TheGreenBowl.Data;
using TheGreenBowl.Models;

namespace TheGreenBowl.Pages.Categories
{
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly TheGreenBowlContext _context;

        public DeleteModel(TheGreenBowlContext context)
        {
            _context = context;
        }

        [BindProperty]
        public tblCategory Category { get; set; }
        
        public int MenusUsingCategory { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Category = await _context.tblCategories
                .Include(c => c.Menus)
                .FirstOrDefaultAsync(c => c.categoryID == id);

            if (Category == null)
            {
                return NotFound();
            }

            MenusUsingCategory = Category.Menus.Count;
            
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Category == null)
            {
                return NotFound();
            }

            // Get the category with its relationships
            var categoryToDelete = await _context.tblCategories
                .Include(c => c.Menus)
                .FirstOrDefaultAsync(c => c.categoryID == Category.categoryID);

            if (categoryToDelete == null)
            {
                return NotFound();
            }

            // Get all menu-category relationships for this category
            var menuCategoryRelationships = await _context.Set<tblMenuCategory>()
                .Where(mc => mc.categoryID == categoryToDelete.categoryID)
                .ToListAsync();

            // Remove the relationships
            foreach (var relationship in menuCategoryRelationships)
            {
                _context.Set<tblMenuCategory>().Remove(relationship);
            }
    
            // Delete the category itself
            _context.tblCategories.Remove(categoryToDelete);
    
            await _context.SaveChangesAsync();
    
            return RedirectToPage("./Index");
        }
    }
}
