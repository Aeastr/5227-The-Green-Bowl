using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TheGreenBowl.Data;
using TheGreenBowl.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TheGreenBowl.Pages.Menu
{
    public class SearchResultsModel : PageModel
    {
        private readonly TheGreenBowlContext _context;
        private readonly ILogger<SearchResultsModel> _logger;

        public SearchResultsModel(TheGreenBowlContext context, ILogger<SearchResultsModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public string q { get; set; }  // Changed from SearchQuery to q to match the form

        public IList<tblMenu> SearchResults { get; set; } = new List<tblMenu>();

        public async Task OnGetAsync()
        {
            if (string.IsNullOrEmpty(q))
            {
                _logger.LogInformation("Search query is empty, returning");
                return;
            }

            var query = q.ToLower().Trim();

            // Find all menu items that match the search query
            var matchingItems = await _context.tblMenuItems
                .Where(mi => 
                    mi.name.ToLower().Contains(query) || 
                    mi.description.ToLower().Contains(query))
                .ToListAsync();

            // Find all menus that have matching items or match the query directly
            var menus = await _context.tblMenus
                .Include(m => m.MenuItems)
                .ThenInclude(mi => mi.menuItem)
                .Include(m => m.Categories)
                .ThenInclude(mc => mc.Category)
                .Where(m => 
                    m.name.ToLower().Contains(query) || 
                    m.description.ToLower().Contains(query) ||
                    m.MenuItems.Any(mi => matchingItems.Select(item => item.itemID).Contains(mi.menuItem.itemID)))
                .ToListAsync();

            // Filter the menu items for each menu to only include relevant items
            foreach (var menu in menus)
            {
                menu.MenuItems = menu.MenuItems
                    .Where(mi => matchingItems.Any(item => item.itemID == mi.menuItem.itemID))
                    .ToList();
            }

            SearchResults = menus;
        }

    }

    
}
