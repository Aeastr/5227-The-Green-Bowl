using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheGreenBowl.Data;
using TheGreenBowl.Models;

namespace TheGreenBowl.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class CategoriesController : ControllerBase
    {
        private readonly TheGreenBowlContext _context;

        public CategoriesController(TheGreenBowlContext context)
        {
            _context = context;
        }

        // GET: api/Categories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<tblCategory>> GetCategory(int id)
        {
            var category = await _context.tblCategories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return category;
        }

        // PUT: api/Categories/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(int id, tblCategory category)
        {
            if (id != category.categoryID)
            {
                return BadRequest();
            }

            _context.Entry(category).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Categories
        [HttpPost]
        public async Task<ActionResult<tblCategory>> PostCategory(tblCategory category)
        {
            _context.tblCategories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCategory", new { id = category.categoryID }, category);
        }

        private bool CategoryExists(int id)
        {
            return _context.tblCategories.Any(e => e.categoryID == id);
        }
    }
}
