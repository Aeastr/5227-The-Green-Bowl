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
    public class EditModel : PageModel
    {
        private readonly TheGreenBowl.Data.TheGreenBowlContext _context;

        public EditModel(TheGreenBowl.Data.TheGreenBowlContext context)
        {
            _context = context;
        }

        [BindProperty]
        public tblMenu tblMenu { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblmenu =  await _context.Menus.FirstOrDefaultAsync(m => m.MenuId == id);
            if (tblmenu == null)
            {
                return NotFound();
            }
            tblMenu = tblmenu;
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(tblMenu).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!tblMenuExists(tblMenu.MenuId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool tblMenuExists(int id)
        {
            return _context.Menus.Any(e => e.MenuId == id);
        }
    }
}
