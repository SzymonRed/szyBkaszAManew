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
    public class RestaurantReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RestaurantReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: RestaurantReviews
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.RestaurantsReviews.Include(r => r.Restaurant);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: RestaurantReviews/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.RestaurantsReviews == null)
            {
                return NotFound();
            }

            var restaurantReview = await _context.RestaurantsReviews
                .Include(r => r.Restaurant)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (restaurantReview == null)
            {
                return NotFound();
            }

            return View(restaurantReview);
        }

        // GET: RestaurantReviews/Create
        public IActionResult Create()
        {
            ViewData["RestaurantId"] = new SelectList(_context.Restaurants, "Id", "Name");
            return View();
        }

        // POST: RestaurantReviews/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Rating,Comment,RestaurantId")] RestaurantReview restaurantReview)
        {
            if (ModelState.IsValid)
            {
                _context.Add(restaurantReview);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RestaurantId"] = new SelectList(_context.Restaurants, "Id", "Name", restaurantReview.RestaurantId);
            return View(restaurantReview);
        }

        // GET: RestaurantReviews/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.RestaurantsReviews == null)
            {
                return NotFound();
            }

            var restaurantReview = await _context.RestaurantsReviews.FindAsync(id);
            if (restaurantReview == null)
            {
                return NotFound();
            }
            ViewData["RestaurantId"] = new SelectList(_context.Restaurants, "Id", "Name", restaurantReview.RestaurantId);
            return View(restaurantReview);
        }

        // POST: RestaurantReviews/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Rating,Comment,RestaurantId")] RestaurantReview restaurantReview)
        {
            if (id != restaurantReview.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(restaurantReview);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RestaurantReviewExists(restaurantReview.Id))
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
            ViewData["RestaurantId"] = new SelectList(_context.Restaurants, "Id", "Name", restaurantReview.RestaurantId);
            return View(restaurantReview);
        }

        // GET: RestaurantReviews/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.RestaurantsReviews == null)
            {
                return NotFound();
            }

            var restaurantReview = await _context.RestaurantsReviews
                .Include(r => r.Restaurant)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (restaurantReview == null)
            {
                return NotFound();
            }

            return View(restaurantReview);
        }

        // POST: RestaurantReviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.RestaurantsReviews == null)
            {
                return Problem("Entity set 'ApplicationDbContext.RestaurantsReviews'  is null.");
            }
            var restaurantReview = await _context.RestaurantsReviews.FindAsync(id);
            if (restaurantReview != null)
            {
                _context.RestaurantsReviews.Remove(restaurantReview);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RestaurantReviewExists(int id)
        {
          return (_context.RestaurantsReviews?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
