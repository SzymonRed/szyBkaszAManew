using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using szyBka_szAMa.Data;
using szyBka_szAMa.Models;

namespace szyBka_szAMa.Controllers
{
    public class DishReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DishReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DishReviews
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.DishReviews.Include(d => d.Dish);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: DishReviews/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.DishReviews == null)
            {
                return NotFound();
            }

            var dishReview = await _context.DishReviews
                .Include(d => d.Dish)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dishReview == null)
            {
                return NotFound();
            }

            return View(dishReview);
        }

        // GET: DishReviews/Create
        public IActionResult Create()
        {
            ViewData["DishId"] = new SelectList(_context.Dishes, "Id", "Description");
            return View();
        }

        // POST: DishReviews/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Rating,Comment,DishId")] DishReview dishReview)
        {
            if (ModelState.IsValid)
            {
                _context.Add(dishReview);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DishId"] = new SelectList(_context.Dishes, "Id", "Description", dishReview.DishId);
            return View(dishReview);
        }

        // GET: DishReviews/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.DishReviews == null)
            {
                return NotFound();
            }

            var dishReview = await _context.DishReviews.FindAsync(id);
            if (dishReview == null)
            {
                return NotFound();
            }
            ViewData["DishId"] = new SelectList(_context.Dishes, "Id", "Description", dishReview.DishId);
            return View(dishReview);
        }

        // POST: DishReviews/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Rating,Comment,DishId")] DishReview dishReview)
        {
            if (id != dishReview.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dishReview);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DishReviewExists(dishReview.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["DishId"] = new SelectList(_context.Dishes, "Id", "Description", dishReview.DishId);
            return View(dishReview);
        }

        // GET: DishReviews/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.DishReviews == null)
            {
                return NotFound();
            }

            var dishReview = await _context.DishReviews
                .Include(d => d.Dish)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dishReview == null)
            {
                return NotFound();
            }

            return View(dishReview);
        }

        // POST: DishReviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.DishReviews == null)
            {
                return Problem("Entity set 'ApplicationDbContext.DishReviews'  is null.");
            }
            var dishReview = await _context.DishReviews.FindAsync(id);
            if (dishReview != null)
            {
                _context.DishReviews.Remove(dishReview);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DishReviewExists(int id)
        {
          return (_context.DishReviews?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
