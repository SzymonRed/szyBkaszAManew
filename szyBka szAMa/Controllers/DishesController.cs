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
    public class DishesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DishesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Dishes
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Dishes.Include(d => d.Restaurant);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Dishes/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var dish = await _context.Dishes
                .Include(d => d.Restaurant)  // Wczytujemy również informacje o restauracji
                .FirstOrDefaultAsync(d => d.Id == id);
            
            var user = HttpContext.Session.GetString("LoggedInUserRole");


            if (dish == null)
            {
                return NotFound();
            }

            ViewBag.Role = user;

            return View(dish);
        }

        // GET: Dishes/Create
        public IActionResult Create(int restaurantId)
        {
            // Przekazujemy Id restauracji do formularza, aby przywiązane było do tego dania
            ViewBag.RestaurantId = restaurantId;
            
            return View();
        }

        // POST: Dishes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Dish dish, IFormFile photo)
        {
            Console.WriteLine($"Dish Name: {dish.Name}, Price: {dish.Price}, RestaurantId: {dish.RestaurantId}");
            if (ModelState.IsValid)
            {
                
                // Jeśli plik został przesłany
                if (photo != null)
                {
                    // Ustalamy ścieżkę do zapisu pliku
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", photo.FileName);

                    // Zapisujemy plik na serwerze
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await photo.CopyToAsync(stream);
                    }

                    // Przypisujemy ścieżkę do zdjęcia
                    dish.Photo = "/images/" + photo.FileName;
                }
                // Sprawdzamy, czy RestaurantId jest poprawne
                if (dish.RestaurantId == 0)
                {
                    return BadRequest("RestaurantId is required.");
                }

                // Dodajemy nowe danie do bazy
                _context.Add(dish);
                await _context.SaveChangesAsync();

                // Po zapisaniu przekierowujemy z powrotem do widoku menu tej restauracji
                return RedirectToAction("Menu", "Restaurants", new { id = dish.RestaurantId });
            }

            // Jeśli model nie jest poprawny, zwróć formularz z błędami
            return View(dish);
        }

        // GET: Dishes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Dishes == null)
            {
                return NotFound();
            }

            var dish = await _context.Dishes.FindAsync(id);
            if (dish == null)
            {
                return NotFound();
            }
            ViewData["RestaurantId"] = new SelectList(_context.Restaurants, "Id", "Name", dish.RestaurantId);
            return View(dish);
        }

        // POST: Dishes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Photo,Price,DishCategory,RestaurantId")] Dish dish)
        {
            if (id != dish.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dish);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DishExists(dish.Id))
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
            ViewData["RestaurantId"] = new SelectList(_context.Restaurants, "Id", "Name", dish.RestaurantId);
            return View(dish);
        }
        
        [HttpGet]
        public IActionResult EditDish(int id)
        {
            var dish = _context.Dishes.Find(id);
            if (dish == null)
            {
                return NotFound();
            }

            var categories = new[]
                {
                    DishCategory.SideDish,
                    DishCategory.MainDish,
                    DishCategory.SoftDrink,
                    DishCategory.Drink
                }
                .Select(category => new SelectListItem
                {
                    Value = category.ToString(),
                    Text = category.ToString()
                })
                .ToList();
            
            ViewBag.Categories = categories;

            return View(dish);
        }

       [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDish(int id, [Bind("Id,Name,Description,Price,DishCategory,RestaurantId,Photo")] Dish dish, IFormFile newPhoto)
        {
            if (id != dish.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Pobierz istniejący rekord z bazy danych
                    var existingDish = await _context.Dishes.FirstOrDefaultAsync(d => d.Id == id);

                    if (existingDish == null)
                    {
                        return NotFound();
                    }

                    // Aktualizacja pól edytowalnych
                    existingDish.Name = dish.Name;
                    existingDish.Description = dish.Description;
                    existingDish.Price = dish.Price;
                    existingDish.DishCategory = dish.DishCategory;

                    // Jeśli nie przesłano nowego zdjęcia, zachowaj stare
                    if (newPhoto != null)
                    {
                        // Ustalamy ścieżkę do zapisu pliku
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

                        // Tworzymy folder, jeśli nie istnieje
                        Directory.CreateDirectory(uploadsFolder);

                        // Generujemy unikalną nazwę pliku, aby uniknąć nadpisania istniejących plików
                        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(newPhoto.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        // Zapisujemy plik na serwerze
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await newPhoto.CopyToAsync(stream);
                        }

                        // Zaktualizuj ścieżkę zdjęcia w obiekcie dania
                        existingDish.Photo = $"/images/+{uniqueFileName}";
                    }
                    else
                    {
                        // Jeśli nowe zdjęcie nie zostało przesłane, zachowaj stare zdjęcie
                        existingDish.Photo = existingDish.Photo;
                    }

                    _context.Update(existingDish);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Menu", "Restaurants", new { id = dish.RestaurantId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DishExists(dish.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Ponownie załaduj kategorie w przypadku błędu
            ViewBag.Categories = Enum.GetValues(typeof(DishCategory))
                .Cast<DishCategory>()
                .Select(dc => new SelectListItem
                {
                    Text = dc.ToString(),
                    Value = dc.ToString()
                })
                .ToList();

            return RedirectToAction("Menu", "Restaurants");
        }
                
        
        // GET: Dishes/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var dish = _context.Dishes.FirstOrDefault(d => d.Id == id);
            if (dish == null)
            {
                return NotFound();
            }

            // Usuwanie dania
            _context.Dishes.Remove(dish);
            _context.SaveChanges();

            // Przekierowanie po usunięciu
            return RedirectToAction("Menu", "Restaurants", new { id = dish.RestaurantId });
        }

        // POST: Dishes/Delete/5
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Dishes == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Dishes'  is null.");
            }
            var dish = await _context.Dishes.FindAsync(id);
            if (dish != null)
            {
                _context.Dishes.Remove(dish);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DishExists(int id)
        {
          return (_context.Dishes?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
