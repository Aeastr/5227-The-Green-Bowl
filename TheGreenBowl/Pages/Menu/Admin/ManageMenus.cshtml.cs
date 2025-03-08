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
    public class ManageMenusModel : PageModel
    {
        private readonly TheGreenBowlContext _context;

        public ManageMenusModel(TheGreenBowlContext context)
        {
            _context = context;
        }

        public IList<MenuViewModel> Menus { get; set; } = new List<MenuViewModel>();
        public List<SelectListItem> AvailableCategories { get; set; } = new List<SelectListItem>();

        public async Task OnGetAsync()
        {
            // Load all menus with their categories and count of items
            Menus = await _context.tblMenus
                .Include(m => m.Categories)
                .ThenInclude(mc => mc.Category)
                .Include(m => m.Menu_MenuItems)
                .Select(menu => new MenuViewModel
                {
                    menuID = menu.menuID,
                    name = menu.name,
                    description = menu.description,
                    Categories = menu.Categories.Select(mc => mc.Category).ToList(),
                    TotalItems = menu.Menu_MenuItems.Count
                })
                .ToListAsync();

            // Load all available categories for the edit form
            AvailableCategories = await _context.tblCategories
                .Select(c => new SelectListItem
                {
                    Value = c.categoryID.ToString(),
                    Text = c.name
                })
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostUpdateMenuAsync([FromBody] UpdateMenuRequest request)
        {
            if (!ModelState.IsValid)
            {
                return new JsonResult(new { success = false, message = "Invalid model state" });
            }
        
            // Start a transaction to ensure all changes are applied together
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Get the existing menu with its categories
                var existingMenu = await _context.tblMenus
                    .Include(m => m.Categories)
                    .ThenInclude(mc => mc.Category)
                    .FirstOrDefaultAsync(m => m.menuID == request.menuID);
                
                if (existingMenu == null)
                {
                    return new JsonResult(new { success = false, message = "Menu not found" });
                }
                
                // Update basic menu properties
                existingMenu.name = request.name;
                existingMenu.description = request.description;
                
                // Remove categories that are no longer selected
                var categoriesToRemove = existingMenu.Categories
                    .Where(mc => !request.selectedCategoryIds.Contains(mc.categoryID))
                    .ToList();
                    
                foreach (var category in categoriesToRemove)
                {
                    existingMenu.Categories.Remove(category);
                }
                
                // Add newly selected categories
                var existingCategoryIds = existingMenu.Categories
                    .Select(mc => mc.categoryID)
                    .ToList();
                    
                var categoriesToAdd = request.selectedCategoryIds
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
                
                // Create updated menu view model to return to the client
                var updatedMenu = new MenuViewModel
                {
                    menuID = existingMenu.menuID,
                    name = existingMenu.name,
                    description = existingMenu.description,
                    Categories = existingMenu.Categories.Select(mc => mc.Category).ToList(),
                    TotalItems = existingMenu.Menu_MenuItems?.Count ?? 0
                };
                
                return new JsonResult(new { 
                    success = true,
                    updatedMenu = updatedMenu
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostDeleteMenuAsync([FromBody] DeleteMenuRequest request)
        {
            if (!ModelState.IsValid)
            {
                return new JsonResult(new { success = false, message = "Invalid model state" });
            }

            // Start a transaction to ensure all changes are applied together
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Get the existing menu with its categories and menu items
                var existingMenu = await _context.tblMenus
                    .Include(m => m.Categories)
                    .Include(m => m.Menu_MenuItems)
                    .FirstOrDefaultAsync(m => m.menuID == request.menuID);
                
                if (existingMenu == null)
                {
                    return new JsonResult(new { success = false, message = "Menu not found" });
                }
                
                // Remove all associated categories
                _context.tblMenuCategory.RemoveRange(existingMenu.Categories);
                
                // Remove all associated menu items
                _context.tblMenu_MenuItems.RemoveRange(existingMenu.Menu_MenuItems);
                
                // Remove the menu itself
                _context.tblMenus.Remove(existingMenu);
                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }
        
        public async Task<IActionResult> OnPostCreateMenuAsync([FromBody] CreateMenuRequest request)
        {
            if (!ModelState.IsValid)
            {
                return new JsonResult(new { success = false, message = "Invalid model state" });
            }

            // Start a transaction to ensure all changes are applied together
            using var transaction = await _context.Database.BeginTransactionAsync();
    
            try
            {
                // Create new menu
                var newMenu = new tblMenu
                {
                    name = request.name,
                    description = request.description,
                    Categories = new List<tblMenuCategory>()
                };
        
                _context.tblMenus.Add(newMenu);
                await _context.SaveChangesAsync();
        
                // Add selected categories
                foreach (var categoryId in request.selectedCategoryIds)
                {
                    newMenu.Categories.Add(new tblMenuCategory
                    {
                        menuID = newMenu.menuID,
                        categoryID = categoryId
                    });
                }
        
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
        
                var newMenuViewModel = new MenuViewModel
                {
                    menuID = newMenu.menuID,
                    name = newMenu.name,
                    description = newMenu.description,
                    Categories = newMenu.Categories.Select(mc => _context.tblCategories.Find(mc.categoryID)).ToList(),
                    TotalItems = 0
                };

                return new JsonResult(new { 
                    success = true,
                    newMenu = newMenuViewModel
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public class CreateMenuRequest
        {
            public string name { get; set; }
            public string description { get; set; }
            public List<int> selectedCategoryIds { get; set; } = new List<int>();
        }

    }

    public class UpdateMenuRequest
    {
        public int menuID { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public List<int> selectedCategoryIds { get; set; } = new List<int>();
    }

    public class DeleteMenuRequest
    {
        public int menuID { get; set; }
    }

}
