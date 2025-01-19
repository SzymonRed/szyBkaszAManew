using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using szyBka_szAMa.Data;
using szyBka_szAMa.Models;

namespace szyBka_szAMa.Controllers
{
    public class AddressesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AddressesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Addresses
        public async Task<IActionResult> Index()
        {
            var addresses = await _context.Adresses.ToListAsync();
            return View(addresses);
        }

        // GET: Addresses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Adresses == null)
            {
                return NotFound();
            }

            var address = await _context.Adresses.FirstOrDefaultAsync(m => m.Id == id);
            if (address == null)
            {
                return NotFound();
            }

            return View(address);
        }

        // GET: Addresses/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Addresses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,City,Street,ZipCode,Building,Apartment,Email,Phone")] Address address)
        {
            if (ModelState.IsValid)
            {
                _context.Add(address);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(address);
        }

        // GET: Address/Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            var address = await _context.Adresses.FindAsync(id);
            if (address == null)
            {
                return NotFound();
            }

            // Pobranie ID restauracji powiązanej z tym adresem
            var restaurant = await _context.Restaurants.FirstOrDefaultAsync(r => r.AddressId == id);
            if (restaurant == null)
            {
                return NotFound();
            }

            ViewBag.RestaurantId = restaurant.Id;
            return View(address);
        }

        // POST: Address/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Address address, int restaurantId)
        {
            if (id != address.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(address);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Adresses.Any(a => a.Id == id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "Restaurants", new { id = restaurantId });
            }
            ViewBag.RestaurantId = restaurantId;
            return View(address);
        }

        // GET: Addresses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Adresses == null)
            {
                return NotFound();
            }

            var address = await _context.Adresses.FirstOrDefaultAsync(m => m.Id == id);
            if (address == null)
            {
                return NotFound();
            }

            return View(address);
        }

        // POST: Addresses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Adresses == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Addresses' is null.");
            }

            var address = await _context.Adresses.FindAsync(id);
            if (address != null)
            {
                _context.Adresses.Remove(address);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AddressExists(int id)
        {
            return _context.Adresses.Any(e => e.Id == id);
        }
    }
}