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
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
              return _context.Users != null ? 
                          View(await _context.Users.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Users'  is null.");
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }
        
        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users.Include(u => u.Address).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            // Pobierz RestaurantId i AddressId z sesji
            var restaurantId = HttpContext.Session.GetInt32("LoggedInUserRestaurantId");
            var addressId = user.AddressId;

            // Przekaż wartości do widoku
            ViewData["RestaurantId"] = restaurantId;
            ViewData["AddressId"] = addressId;

            var roles = new[]
                {
                    UserRole.RestaurantCook,
                    UserRole.RestaurantDelivery,
                    UserRole.RestaurantWaiter
                }
                .Select(role => new SelectListItem
                {
                    Value = role.ToString(),
                    Text = role.ToString()
                })
                .ToList();
    
            ViewBag.Roles = roles;
            return View(user);
        }
        
     [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,Password,UserName,Role,AddressId,Address")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Pobierz istniejącego użytkownika z bazy danych wraz z powiązanym adresem
                    var existingUser = await _context.Users
                        .Include(u => u.Address) // Dołącz adres użytkownika
                        .FirstOrDefaultAsync(u => u.Id == id);

                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    // Aktualizacja danych użytkownika
                    existingUser.FirstName = user.FirstName;
                    existingUser.LastName = user.LastName;
                    existingUser.UserName = user.UserName;
                    existingUser.Role = user.Role;

                    // Jeśli użytkownik wprowadził nowe hasło, zaktualizuj je (dodaj ewentualną logikę szyfrowania hasła)
                    if (!string.IsNullOrEmpty(user.Password))
                    {
                        existingUser.Password = user.Password; // Zaimplementuj szyfrowanie hasła
                    }

                    // Zaktualizowanie danych adresu (jeśli zmieniono)
                    if (user.Address != null)
                    {
                        var existingAddress = existingUser.Address;
                        existingAddress.City = user.Address.City;
                        existingAddress.Street = user.Address.Street;
                        existingAddress.ZipCode = user.Address.ZipCode;
                        existingAddress.Building = user.Address.Building;
                        existingAddress.Apartment = user.Address.Apartment;
                        existingAddress.Email = user.Address.Email;
                        existingAddress.Phone = user.Address.Phone;

                        // Możesz dodać logikę walidacji dla adresu, jeśli jest to potrzebne
                    }

                    // Zaktualizowanie użytkownika w bazie danych
                    _context.Update(existingUser);
                    await _context.SaveChangesAsync();
                    var userId = HttpContext.Session.GetInt32("LoggedInUserId");
                    var user2 = _context.Users.FirstOrDefault(u => u.Id == userId);

                    
                    var restaurantId = user2.RestaurantId.Value;

                    return RedirectToAction("Employees", "Restaurants", new { restaurantId });
                    
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Jeśli dane są niepoprawne, ponownie załaduj dane do widoku
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Users == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Users'  is null.");
            }
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            
            var userId = HttpContext.Session.GetInt32("LoggedInUserId");
    
            // Pobierz użytkownika z bazy danych
            var user2 = _context.Users.FirstOrDefault(u => u.Id == userId);
    
            if (user2 == null)
            {
                // Obsługa sytuacji, gdy użytkownik nie istnieje
                return RedirectToAction("Index", "Home");
            }
    
            // Jeśli użytkownik ma przypisaną restaurację
            if (user2.RestaurantId.HasValue)
            {
                // Pobierz ID restauracji
                var restaurantId = user2.RestaurantId.Value;

                // Możesz teraz wykonać operacje z restauracją, np. przekierować do szczegółów
                return RedirectToAction("Employees", "Restaurants", new { id = restaurantId });
            }

            return RedirectToAction("Details", "Restaurants");
        }

        private bool UserExists(int id)
        {
          return (_context.Users?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        
        // Akcja GET do wyświetlania formularza logowania
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "E-mail i hasło są wymagane.");
                return View();
            }

            // Szukamy użytkownika na podstawie e-maila w tabeli Address powiązanego z restauracją
            var user = _context.Users
                .FirstOrDefault(u => u.Address.Email == email && u.Password == password);

            if (user != null)
            {
                // Ustawienie sesji użytkownika
                HttpContext.Session.SetInt32("LoggedInUserId", user.Id);
                HttpContext.Session.SetString("LoggedInUserRole", user.Role.ToString());

                // Przekierowanie do strony głównej po pomyślnym logowaniu
                return RedirectToAction("Index", "Home");
            }
            else
            {
                // Wyświetlenie komunikatu o błędzie
                ViewData["ErrorMessage"] = "Nieprawidłowy e-mail lub hasło.";
                return View();
            }
        }
        
        
        // Akcja wyświetlająca formularz dodawania nowego pracownika
        [HttpGet]
        public IActionResult CreateNewEmployee(int restaurantId)
        {
            // Pobieramy restaurację na podstawie ID
            var restaurant = _context.Restaurants.FirstOrDefault(r => r.Id == restaurantId);
            if (restaurant == null)
            {
                return NotFound();
            }

          
            var roles = new[]
                {
                    UserRole.RestaurantCook,
                    UserRole.RestaurantDelivery,
                    UserRole.RestaurantWaiter
                }
                .Select(role => new SelectListItem
                {
                    Value = role.ToString(),
                    Text = role.ToString()
                })
                .ToList();

            // Przygotowanie modelu użytkownika
            var user = new User
            {
                RestaurantId = restaurantId
            };

            // Przekazanie listy ról do widoku
            ViewBag.Roles = roles;

            return View(user);
        }

        // Akcja zapisująca nowego pracownika
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateNewEmployee(User model, Address address)
        {
            if (ModelState.IsValid)
            {
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
                
                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Password = model.Password,
                    UserName = model.UserName,
                    Role = model.Role,  // Przypisujemy rolę z formularza
                    AddressId = newAddress.Id,
                    RestaurantId = model.RestaurantId
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                return RedirectToAction("Details", "Restaurants", new { id = model.RestaurantId });
            }

            // Jeśli model nie jest poprawny, ponownie renderujemy formularz
            return View(model);
        }
        
         [HttpGet]
        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetInt32("LoggedInUserId");

            if (userId.HasValue)
            {
                
                return RedirectToAction("Index", "Home"); 
            }
            return View(); 
        }

       [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(User model, Address address)
        {
            if (ModelState.IsValid)
            {
                
                var existingUser = _context.Users.FirstOrDefault(u => u.UserName == model.UserName);

                if (existingUser != null)
                {
                    ModelState.AddModelError("", "Użytkownik o tej nazwie już istnieje.");
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
                    Role = UserRole.Client,
                    AddressId = newAddress.Id
                };

                _context.Users.Add(newUser);
                _context.SaveChanges();

                TempData["Success"] = "Użytkownik został pomyślnie zarejestrowany.";
                return RedirectToAction("Login", "Users");
            }

            return View(model);
        }
    }
}
