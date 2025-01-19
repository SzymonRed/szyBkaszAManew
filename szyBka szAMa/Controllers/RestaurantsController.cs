using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using szyBka_szAMa.Data;
using szyBka_szAMa.Models;

namespace szyBka_szAMa.Controllers
{
    public class RestaurantsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RestaurantsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Restaurants
        public ActionResult Index()
        {
            var restaurants = _context.Restaurants.Include("Address").ToList();
            return View(restaurants);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

        // GET: Restaurants/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                // Pobieranie szczegółów restauracji wraz z powiązanymi danymi
                var restaurant = await _context.Restaurants
                    .Include(r => r.Address)
                    .Include(r => r.Employees) // Załaduj pracowników
                    .Include(r => r.Reviews)
                    .Include(r => r.WorkHours)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (restaurant == null)
                {
                    return NotFound();
                }

                // Przekazanie restauracji do widoku
                return View(restaurant);
            }
            catch (Exception ex)
            {
                // Obsługa błędów (opcjonalne logowanie)
                Console.WriteLine($"Error loading restaurant details: {ex.Message}");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
        
        public async Task<IActionResult> Employees(int restaurantId)
        {
            var restaurant = await _context.Restaurants
                .Include(r => r.Employees)
                .FirstOrDefaultAsync(r => r.Id == restaurantId);

            if (restaurant == null)
            {
                return NotFound();
            }
            
            ViewBag.RestaurantId = restaurantId;
            return View(restaurant.Employees);
        }
        
        [HttpGet]
        public IActionResult RegisterRestaurantAdministrator()
        {
            var userId = HttpContext.Session.GetInt32("LoggedInUserId");
            var userRole = HttpContext.Session.GetString("LoggedInUserRole");

            if (userId.HasValue && userRole == UserRole.RestaurantAdministrator.ToString())
            {
                // Jeśli użytkownik jest już zalogowany i ma rolę, przekieruj do innego widoku
                return RedirectToAction("RegisterRestaurant", "Restaurants"); // Przekierowanie do widoku dla administratora
            }
            return View(); // Zwraca widok do rejestracji administratora restauracji
        }

       [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegisterRestaurantAdministrator(User model, Address address)
        {
            if (ModelState.IsValid)
            {
                // Sprawdź, czy użytkownik o podanej nazwie już istnieje
                var existingUser = _context.Users.FirstOrDefault(u => u.UserName == model.UserName);

                if (existingUser != null)
                {
                    // Jeśli użytkownik istnieje, sprawdź jego rolę
                    if (existingUser.Role != UserRole.RestaurantAdministrator)
                    {
                        ModelState.AddModelError("", "Tylko użytkownik z rolą administratora restauracji może być zarejestrowany.");
                        return View(model);
                    }

                    ModelState.AddModelError("", "Administrator restauracji o tej nazwie już istnieje.");
                    return View(model);
                }
                
                var newAddress = new Address
                {
                    City = address.City,
                    Street = address.Street,
                    ZipCode = address.ZipCode,
                    Building = address.Building,
                    Apartment = address.Apartment,
                    Email = address.Email,
                    Phone = address.Phone
                };

                _context.Add(newAddress);
                _context.SaveChanges();

                // Tworzenie nowego użytkownika
                var newUser = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    UserName = model.UserName,
                    Password = model.Password,
                    Role = UserRole.RestaurantAdministrator,
                    AddressId = newAddress.Id
                };

                _context.Users.Add(newUser);
                _context.SaveChanges();

                TempData["Success"] = "Administrator restauracji został pomyślnie zarejestrowany.";
                return RedirectToAction("LoginAdminRestaurant", "Restaurants");
            }

            return View(model);
        }
        
        [HttpGet]
        public IActionResult RegisterRestaurant()
        {
            return View();
        }

       [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegisterRestaurant(Restaurant model, Address address)
        {
            var userId = HttpContext.Session.GetInt32("LoggedInUserId");
            var userRole = HttpContext.Session.GetString("LoggedInUserRole");

            if (!(userId.HasValue || userRole == UserRole.RestaurantAdministrator.ToString()))
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                // Tworzenie nowego adresu
                var newAddress = new Address
                {
                    City = address.City,
                    Street = address.Street,
                    ZipCode = address.ZipCode,
                    Building = address.Building,
                    Apartment = address.Apartment,
                    Email = address.Email,
                    Phone = address.Phone
                };

                // Dodaj adres do bazy danych
                _context.Add(newAddress);
                _context.SaveChanges(); // Zapisz adres

                // Odczytaj zapisany adres po zapisaniu
                var savedAddress = _context.Adresses.FirstOrDefault(a => a.City == newAddress.City && a.Street == newAddress.Street);

                if (savedAddress != null)
                {
                    // Tworzenie nowej restauracji
                    var restaurant = new Restaurant
                    {
                        Name = model.Name,
                        AddressId = savedAddress.Id // Przypisanie Id zapisowego adresu
                    };

                    // Dodaj restaurację do bazy danych
                    _context.Add(restaurant);
                    _context.SaveChanges(); // Zapisz restaurację

                    // Zaktualizowanie użytkownika, aby przypisać RestaurantId
                    var currentUser = _context.Users.FirstOrDefault(u => u.Id == userId.Value);
                    if (userId.HasValue)
                    {
                        currentUser.RestaurantId =
                            restaurant.Id; // Przypisz Id nowo utworzonej restauracji do użytkownika
                        _context.Update(currentUser); // Zaktualizuj użytkownika w bazie danych
                        _context.SaveChanges(); // Zapisz
                    }
                    
                    // Przekierowanie do dodawania godzin pracy restauracji
                    return RedirectToAction("AddWorkHours", "WorkHours", new { restaurantId = restaurant.Id });
                }
            }
            // Jeśli dane są nieprawidłowe, wyświetl formularz ponownie
            return View();
        }
        
        public async Task<IActionResult> Menu(int id)
        {
            var user = HttpContext.Session.GetString("LoggedInUserRole");
            var restaurant = await _context.Restaurants
                .Include(r => r.Dishes) // Dołączamy dania restauracji
                .FirstOrDefaultAsync(r => r.Id == id);

            if (restaurant == null)
            {
                return NotFound();
            }

            ViewBag.Role = user;

            // Przekazujemy do widoku restaurację, a w tym kontekście także listę dań
            return View(restaurant);
        }
        

        // GET: Restaurants/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Restaurants == null)
            {
                return NotFound();
            }

            var restaurant = await _context.Restaurants.FindAsync(id);
            if (restaurant == null)
            {
                return NotFound();
            }
            ViewData["AddressId"] = new SelectList(_context.Adresses, "Id", "Building", restaurant.AddressId);
            return View(restaurant);
        }

        // POST: Restaurants/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,AddressId")] Restaurant restaurant)
        {
            if (id != restaurant.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(restaurant);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RestaurantExists(restaurant.Id))
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
            ViewData["AddressId"] = new SelectList(_context.Adresses, "Id", "Building", restaurant.AddressId);
            return View(restaurant);
        }

        // GET: Restaurants/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Restaurants == null)
            {
                return NotFound();
            }

            var restaurant = await _context.Restaurants
                .Include(r => r.Address)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (restaurant == null)
            {
                return NotFound();
            }

            return View(restaurant);
        }

        // POST: Restaurants/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Restaurants == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Restaurants'  is null.");
            }
            var restaurant = await _context.Restaurants.FindAsync(id);
            if (restaurant != null)
            {
                _context.Restaurants.Remove(restaurant);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RestaurantExists(int id)
        {
          return (_context.Restaurants?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        
        public IActionResult LoginAdminRestaurant()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LoginAdminRestaurant(string email, string password)
        {
            var address = _context.Adresses.FirstOrDefault(a => a.Email == email);
            if (address == null)
            {
                ViewData["LoginError"] = "Podany email nie istnieje.";
                return View();
            }

            // Znajdź uzytkownika przypisanego do tego adresu
            var user = _context.Users.FirstOrDefault(r => r.AddressId == address.Id);
            if (user == null)
            {
                ViewData["LoginError"] = "Brak uzytkownika powiązanej z podanym emailem.";
                return View();
            }
            HttpContext.Session.SetInt32("LoggedInUserId", user.Id);
            HttpContext.Session.SetString("LoggedInUserRole", user.Role.ToString());
            
                // Możesz tu zapisać dane do sesji lub ciasteczek
                HttpContext.Session.SetString("RestaurantId", user.Id.ToString());
                return RedirectToAction("LoginRestaurant", "Restaurants"); // Przykładowa strona po logowaniu
        }
        
        [HttpGet]
        public IActionResult LoginRestaurant()
        {
            return View();
        }

        [HttpPost]
        public IActionResult LoginRestaurant(string email, string password)
        {
            var address = _context.Adresses.FirstOrDefault(a => a.Email == email);

            var restaurant = _context.Restaurants.FirstOrDefault(r => r.AddressId == address.Id);

            // Sprawdź, czy użytkownik jest już zalogowany
            var loggedInUserId = HttpContext.Session.GetInt32("LoggedInUserId");
            if (loggedInUserId != null)
            {
                // Znajdź zalogowanego użytkownika
                var loggedInUser = _context.Users.FirstOrDefault(u => u.Id == loggedInUserId);

                if (loggedInUser != null && loggedInUser.RestaurantId != restaurant.Id)
                {
                    ViewData["LoginError"] = "Nie możesz zalogować się do restauracji, która nie jest powiązana z Twoim kontem.";
                    return View();
                }
            }
            else
            {
                return RedirectToAction("LoginAdminRestaurant", "Restaurants");
            }
            // Znajdź adres z podanym emailem
            if (address == null)
            {
                ViewData["LoginError"] = "Podany email nie istnieje.";
                return View();
            }

            // Znajdź restaurację przypisaną do tego adresu
            if (restaurant == null)
            {
                ViewData["LoginError"] = "Brak restauracji powiązanej z podanym emailem.";
                return View();
            }

            // Znajdź administratora restauracji
            var admin = _context.Users.FirstOrDefault(u => u.RestaurantId == restaurant.Id && u.Role == UserRole.RestaurantAdministrator && u.Password == password);
            if (admin == null)
            {
                ViewData["LoginError"] = "Nieprawidłowe hasło administratora.";
                return View();
            }

            // Zalogowanie użytkownika (ustawienie sesji)
            HttpContext.Session.SetString("UserId", admin.Id.ToString());
            HttpContext.Session.SetString("UserRole", admin.Role.ToString());

            return RedirectToAction("Details", "Restaurants", new { id = restaurant.Id }); // Przekierowanie do panelu restauracji
        }
        
        public IActionResult Orders(int restaurantId)
        {
            var orders = _context.Orders
                .Where(o => o.RestaurantId == restaurantId)
                .OrderByDescending(o => o.TimeOrdered) // Opcjonalnie, żeby posortować po dacie
                .ToList();
            
            ViewBag.RestaurantId = restaurantId;
            return View(orders);
        }
    }
}
