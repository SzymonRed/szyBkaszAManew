using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using szyBka_szAMa.Data;
using szyBka_szAMa.Models;
using DayOfWeek = szyBka_szAMa.Models.DayOfWeek;

namespace szyBka_szAMa.Controllers
{
    public class WorkHoursController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WorkHoursController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: WorkHours
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.WorkHours.Include(w => w.Restaurant);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: WorkHours/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.WorkHours == null)
            {
                return NotFound();
            }

            var workHour = await _context.WorkHours
                .Include(w => w.Restaurant)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (workHour == null)
            {
                return NotFound();
            }

            return View(workHour);
        }

        // GET: WorkHours/Create
        [HttpGet]
        public IActionResult AddWorkHours(int restaurantId)
        {
            var restaurant = _context.Restaurants.FirstOrDefault(r => r.Id == restaurantId);

            if (restaurant == null)
            {
                return NotFound("Restaurant not found");
            }

            var workHours = Enum.GetValues(typeof(DayOfWeek))
                .Cast<DayOfWeek>()
                .Select(day => new WorkHour
                {
                    DayOfWeek = day,
                    OpenHour = DateTime.MinValue,  // Domyślne wartości
                    CloseHour = DateTime.MinValue  // Domyślne wartości
                })
                .ToList();

            ViewBag.RestaurantName = restaurant.Name;
            ViewBag.RestaurantId = restaurantId;

            return View(workHours);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddWorkHours(int restaurantId, List<WorkHour> workHours)
        {
            var restaurant = _context.Restaurants.FirstOrDefault(r => r.Id == restaurantId);

            if (restaurant == null)
            {
                return NotFound("Restaurant not found");
            }

            if (!ModelState.IsValid)
            {
                // Loguj błędy walidacji
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                foreach (var error in errors)
                {
                    Console.WriteLine(error); // Możesz użyć loggera zamiast konsoli
                }
        
                // Ponownie wyświetl formularz z błędami
                ViewBag.RestaurantName = restaurant?.Name ?? "Unknown";
                return View(workHours);
            }

            // Loguj przesłane dane dla diagnozy
            Console.WriteLine($"RestaurantId: {restaurantId}");
            foreach (var hour in workHours)
            {
                Console.WriteLine($"{hour.DayOfWeek}: {hour.OpenHour} - {hour.CloseHour}");
            }

            // Przypisanie RestaurantId do każdej godziny pracy
            foreach (var workHour in workHours)
            {
                workHour.RestaurantId = restaurantId;
                _context.WorkHours.Add(workHour);
            }

            _context.SaveChanges();
            return RedirectToAction("LoginRestaurant", "Restaurants", new { id = restaurantId });
        }

        // POST: WorkHours/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // GET: WorkHours/Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            var workHour = await _context.WorkHours.FindAsync(id);
            if (workHour == null)
            {
                return NotFound();
            }

            return View(workHour);
        }

        // POST: WorkHours/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, WorkHour workHour)
        {
            if (id != workHour.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(workHour);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.WorkHours.Any(wh => wh.Id == id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction("Details", "Restaurants", new { id = workHour.RestaurantId });
            }

            return View(workHour);
        }

        // GET: WorkHours/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.WorkHours == null)
            {
                return NotFound();
            }

            var workHour = await _context.WorkHours
                .Include(w => w.Restaurant)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (workHour == null)
            {
                return NotFound();
            }

            return View(workHour);
        }

        // POST: WorkHours/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.WorkHours == null)
            {
                return Problem("Entity set 'ApplicationDbContext.WorkHours'  is null.");
            }
            var workHour = await _context.WorkHours.FindAsync(id);
            if (workHour != null)
            {
                _context.WorkHours.Remove(workHour);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WorkHourExists(int id)
        {
          return (_context.WorkHours?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
