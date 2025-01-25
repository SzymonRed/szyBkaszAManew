using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using szyBka_szAMa.Data;
using szyBka_szAMa.Models;

namespace szyBka_szAMa.Controllers
{
    public class BasketsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BasketsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Baskets
        public ActionResult Index()
        {
            var userName = HttpContext.Session.GetInt32("LoggedInUserId");

            var basket = _context.Baskets
                .Include(b => b.Dishes)
                .ThenInclude(d => d.Restaurant) // Załadowanie restauracji z daniem
                .Include(b => b.User)
                .FirstOrDefault(b => b.User.Id == userName);

            if (basket == null)
            {
                return View(new List<Dish>() as IEnumerable<IGrouping<int, Dish>>); // Pusty koszyk
            }

            // Zgrupowanie dań według Restauracji
            var groupedDishes = basket.Dishes.GroupBy(d => d.RestaurantId).ToList();

            return View(groupedDishes); // Przekazujemy zgrupowane dania
        }

        // GET: Baskets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Baskets == null)
            {
                return NotFound();
            }

            var basket = await _context.Baskets
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (basket == null)
            {
                return NotFound();
            }

            return View(basket);
        }

        // GET: Baskets/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Baskets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId")] Basket basket)
        {
            if (ModelState.IsValid)
            {
                _context.Add(basket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", basket.UserId);
            return View(basket);
        }

        // GET: Baskets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Baskets == null)
            {
                return NotFound();
            }

            var basket = await _context.Baskets.FindAsync(id);
            if (basket == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", basket.UserId);
            return View(basket);
        }

        // POST: Baskets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId")] Basket basket)
        {
            if (id != basket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(basket);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BasketExists(basket.Id))
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
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", basket.UserId);
            return View(basket);
        }

        // GET: Baskets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Baskets == null)
            {
                return NotFound();
            }

            var basket = await _context.Baskets
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (basket == null)
            {
                return NotFound();
            }

            return View(basket);
        }

        // POST: Baskets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Baskets == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Baskets'  is null.");
            }
            var basket = await _context.Baskets.FindAsync(id);
            if (basket != null)
            {
                _context.Baskets.Remove(basket);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BasketExists(int id)
        {
          return (_context.Baskets?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        
        [HttpPost]
        public ActionResult AddToBasket(int dishId, int restaurantId)
        {
            var userName = HttpContext.Session.GetInt32("LoggedInUserId");
            var user = _context.Users.FirstOrDefault(u => u.Id == userName);

            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Znajdź istniejący koszyk użytkownika lub utwórz nowy
            var basket = _context.Baskets.Include(b => b.Dishes)
                .FirstOrDefault(b => b.UserId == user.Id);

            if (basket == null)
            {
                basket = new Basket { UserId = user.Id, User = user };
                _context.Baskets.Add(basket);
            }

            // Dodaj danie do koszyka
            var dish = _context.Dishes.Find(dishId);
            if (dish != null && !basket.Dishes.Contains(dish))
            {
                basket.Dishes.Add(dish);
                _context.SaveChanges();
            }

            return RedirectToAction("Menu", "Restaurants", new { id = restaurantId });
        }
        
        [HttpGet]
        public ActionResult Checkout(int restaurantId)
        {
            var userId = HttpContext.Session.GetInt32("LoggedInUserId");

            // Pobierz wszystkie dania z tej restauracji, które znajdują się w koszyku użytkownika
            //var userId = User.GetId(); // Zakładając, że masz identyfikację użytkownika
            var basket = _context.Baskets
                .Include(b => b.Dishes)
                .FirstOrDefault(b => b.UserId == userId && b.Dishes.Any(d => d.RestaurantId == restaurantId));

            if (basket == null)
            {
                TempData["Message"] = "Your basket is empty for this restaurant.";
                return RedirectToAction("Index", "Restaurants");
            }

            // Utwórz nowe zamówienie
            var order = new Order
            {
                UserId = (int)userId,
                RestaurantId = restaurantId,
                Dishes = basket.Dishes.Where(d => d.RestaurantId == restaurantId).ToList(),
                DeliveryMethod = DeliveryMethod.Delivery, // Można dodać logikę wyboru
                PaymentMethod = PaymentMethod.OnDelivery, // Można dodać logikę wyboru
                Status = OrderStatus.WaitingPayment,
                TimeOrdered = DateTime.Now,
                PaymentComplete = false
            };

            // Zwróć widok z zamówieniem
            return View(order);
        }
        
        [HttpPost]
        public IActionResult RemoveFromBasket(int dishId)
        {
            // Pobierz ID zalogowanego użytkownika z sesji
            var userId = HttpContext.Session.GetInt32("LoggedInUserId");

            // Sprawdź, czy użytkownik jest zalogowany
            if (userId == null)
            {
                return RedirectToAction("Login", "Users"); // Możesz przekierować do logowania, jeśli użytkownik nie jest zalogowany
            }

            // Znajdź koszyk powiązany z użytkownikiem
            var basket = _context.Baskets.Include(b => b.Dishes)
                .FirstOrDefault(b => b.UserId == userId);

            if (basket != null)
            {
                // Znajdź danie, które ma zostać usunięte
                var dishToRemove = basket.Dishes.FirstOrDefault(d => d.Id == dishId);

                if (dishToRemove != null)
                {
                    // Usuń danie z listy Dishes
                    basket.Dishes.Remove(dishToRemove);

                    // Zapisz zmiany w bazie danych
                    _context.SaveChanges();
                }
            }

            // Po usunięciu przekierowanie do widoku koszyka
            return RedirectToAction("Index");
        }

    }
}
