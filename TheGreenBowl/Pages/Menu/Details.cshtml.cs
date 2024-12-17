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
    public class DetailsModel : PageModel
    {
        private readonly TheGreenBowl.Data.TheGreenBowlContext _context;

        public DetailsModel(TheGreenBowl.Data.TheGreenBowlContext context)
        {
            _context = context;
        }

        public tblMenu tblMenu { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblmenu = await _context.Menus.FirstOrDefaultAsync(m => m.MenuId == id);
            if (tblmenu == null)
            {
                return NotFound();
            }
            else
            {
                tblMenu = tblmenu;
            }
            return Page();
        }
    }
}
