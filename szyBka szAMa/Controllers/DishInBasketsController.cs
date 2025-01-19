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
    public class DishInBasketsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DishInBasketsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DishInBaskets
        public async Task<IActionResult> Index()
        {
              return _context.DishInBasket != null ? 
                          View(await _context.DishInBasket.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.DishInBasket'  is null.");
        }

        // GET: DishInBaskets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.DishInBasket == null)
            {
                return NotFound();
            }

            var dishInBasket = await _context.DishInBasket
                .FirstOrDefaultAsync(m => m.BasketId == id);
            if (dishInBasket == null)
            {
                return NotFound();
            }

            return View(dishInBasket);
        }

        // GET: DishInBaskets/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: DishInBaskets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BasketId,DishId,Amount")] DishInBasket dishInBasket)
        {
            if (ModelState.IsValid)
            {
                _context.Add(dishInBasket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(dishInBasket);
        }

        // GET: DishInBaskets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.DishInBasket == null)
            {
                return NotFound();
            }

            var dishInBasket = await _context.DishInBasket.FindAsync(id);
            if (dishInBasket == null)
            {
                return NotFound();
            }
            return View(dishInBasket);
        }

        // POST: DishInBaskets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BasketId,DishId,Amount")] DishInBasket dishInBasket)
        {
            if (id != dishInBasket.BasketId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dishInBasket);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DishInBasketExists(dishInBasket.BasketId))
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
            return View(dishInBasket);
        }

        // GET: DishInBaskets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.DishInBasket == null)
            {
                return NotFound();
            }

            var dishInBasket = await _context.DishInBasket
                .FirstOrDefaultAsync(m => m.BasketId == id);
            if (dishInBasket == null)
            {
                return NotFound();
            }

            return View(dishInBasket);
        }

        // POST: DishInBaskets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.DishInBasket == null)
            {
                return Problem("Entity set 'ApplicationDbContext.DishInBasket'  is null.");
            }
            var dishInBasket = await _context.DishInBasket.FindAsync(id);
            if (dishInBasket != null)
            {
                _context.DishInBasket.Remove(dishInBasket);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DishInBasketExists(int id)
        {
          return (_context.DishInBasket?.Any(e => e.BasketId == id)).GetValueOrDefault();
        }
    }
}
