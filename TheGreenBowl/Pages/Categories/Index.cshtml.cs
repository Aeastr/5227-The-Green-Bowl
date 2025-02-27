using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TheGreenBowl.Data;
using TheGreenBowl.Models;

namespace TheGreenBowl.Pages.Categories
{
    public class IndexModel : PageModel
    {
        private readonly TheGreenBowlContext _context;

        public IndexModel(TheGreenBowlContext context)
        {
            _context = context;
        }

        public class CategoryViewModel
        {
            public int categoryID { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public int MenusCount { get; set; }
        }

        public List<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();

        public async Task OnGetAsync()
        {
            Categories = await _context.tblCategories
                .Include(c => c.Menus)
                .Select(c => new CategoryViewModel
                {
                    categoryID = c.categoryID,
                    name = c.name,
                    description = c.description,
                    MenusCount = c.Menus.Count
                })
                .ToListAsync();
        }
    }
}