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
        private readonly TheGreenBowl.Data.TheGreenBowlContext _context;

        public IndexModel(TheGreenBowl.Data.TheGreenBowlContext context)
        {
            _context = context;
        }

        public IList<tblMenu> tblMenu { get;set; } = default!;

        public async Task OnGetAsync()
        {
            tblMenu = await _context.Menus.ToListAsync();
        }
    }
}
