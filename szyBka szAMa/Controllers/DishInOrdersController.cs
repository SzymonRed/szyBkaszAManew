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
    public class DishInOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DishInOrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DishInOrders
        public async Task<IActionResult> Index()
        {
              return _context.DishesInOrders != null ? 
                          View(await _context.DishesInOrders.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.DishesInOrders'  is null.");
        }

        // GET: DishInOrders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.DishesInOrders == null)
            {
                return NotFound();
            }

            var dishInOrder = await _context.DishesInOrders
                .FirstOrDefaultAsync(m => m.DishId == id);
            if (dishInOrder == null)
            {
                return NotFound();
            }

            return View(dishInOrder);
        }

        // GET: DishInOrders/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: DishInOrders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,DishId,Amount")] DishInOrder dishInOrder)
        {
            if (ModelState.IsValid)
            {
                _context.Add(dishInOrder);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(dishInOrder);
        }

        // GET: DishInOrders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.DishesInOrders == null)
            {
                return NotFound();
            }

            var dishInOrder = await _context.DishesInOrders.FindAsync(id);
            if (dishInOrder == null)
            {
                return NotFound();
            }
            return View(dishInOrder);
        }

        // POST: DishInOrders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,DishId,Amount")] DishInOrder dishInOrder)
        {
            if (id != dishInOrder.DishId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dishInOrder);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DishInOrderExists(dishInOrder.DishId))
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
            return View(dishInOrder);
        }

        // GET: DishInOrders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.DishesInOrders == null)
            {
                return NotFound();
            }

            var dishInOrder = await _context.DishesInOrders
                .FirstOrDefaultAsync(m => m.DishId == id);
            if (dishInOrder == null)
            {
                return NotFound();
            }

            return View(dishInOrder);
        }

        // POST: DishInOrders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.DishesInOrders == null)
            {
                return Problem("Entity set 'ApplicationDbContext.DishesInOrders'  is null.");
            }
            var dishInOrder = await _context.DishesInOrders.FindAsync(id);
            if (dishInOrder != null)
            {
                _context.DishesInOrders.Remove(dishInOrder);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DishInOrderExists(int id)
        {
          return (_context.DishesInOrders?.Any(e => e.DishId == id)).GetValueOrDefault();
        }
    }
}
