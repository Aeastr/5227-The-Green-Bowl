using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TheGreenBowl.Data;
using TheGreenBowl.Models;

namespace TheGreenBowl.Pages.Menu
{
    public class IndexModel : PageModel
    {
        private readonly TheGreenBowlContext _context;

        public IndexModel(TheGreenBowlContext context)
        {
            _context = context;
        }

        public IList<MenuViewModel> Menus { get; set; } = new List<MenuViewModel>();

        public async Task OnGetAsync()
        {
            Menus = await _context.tblMenus
                .Include(m => m.Categories) // Include the relationship with tblMenuCategory
                .ThenInclude(mc => mc.Category) // Load the tblCategory data
                .Include(m => m.MenuItems) // Include the relationship with tblMenu_MenuItem
                .Select(menu => new MenuViewModel
                {
                    menuID = menu.menuID,
                    name = menu.name,
                    description = menu.description,
                    Categories = menu.Categories.Select(mc => mc.Category).ToList(), // Map Categories
                    TotalItems = menu.MenuItems.Count // Count the related menu items
                })
                .ToListAsync();
        }
    }

    public class MenuViewModel
    {
        public int menuID { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public List<tblCategory> Categories { get; set; }
        public int TotalItems { get; set; }
    }
}