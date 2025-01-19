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
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Orders.Include(o => o.Restaurant);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var user = HttpContext.Session.GetString("LoggedInUserRole");

            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Restaurant)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            ViewBag.UserRole = user;

            return View(order);
        }

        public ActionResult Create()
        {
            var userName = HttpContext.Session.GetInt32("LoggedInUserId");
            var user = _context.Users.FirstOrDefault(u => u.Id == userName);
            if (user == null) return RedirectToAction("Index", "Home");

            var basket = _context.Baskets.Include(b => b.Dishes).FirstOrDefault(b => b.UserId == user.Id);
            if (basket == null || !basket.Dishes.Any())
            {
                TempData["Message"] = "Your basket is empty.";
                return RedirectToAction("Index", "Restaurants");
            }

            var order = new Order
            {
                UserId = user.Id,
                Dishes = basket.Dishes.ToList(),
                RestaurantId = basket.Dishes.First().RestaurantId,
                TimeOrdered = DateTime.Now,
                Status = OrderStatus.WaitingPayment
            };

            return View(order);
        }

        // POST: Submit Order
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Order order)
        {
            if (!ModelState.IsValid) return View(order);

            // Ustawienie początkowych wartości
            order.TimeOrdered = DateTime.Now;
            order.Status = OrderStatus.WaitingPayment; // Domyślny status przed płatnością
            if (order.PaymentMethod == PaymentMethod.Online)
            {
                order.PaymentComplete = true;
            }
            else
            {
                order.PaymentComplete = false;
            }

            // Dodanie zamówienia do bazy
            _context.Orders.Add(order);
            _context.SaveChanges();

            // Ustawienie statusu na "Taken" po złożeniu zamówienia
            order.Status = OrderStatus.Taken;
            _context.SaveChanges();

            // Opróżnianie koszyka
            var basket = _context.Baskets.Include(b => b.Dishes).FirstOrDefault(b => b.UserId == order.UserId);
            if (basket != null)
            {
                basket.Dishes.Clear();
                _context.SaveChanges();
            }

            TempData["Message"] = "Your order has been placed successfully!";
            return RedirectToAction("Index", "Restaurants");
        }


        // GET: Orders/Edit/5
        public IActionResult Edit(int id)
        {
            var order = _context.Orders
                .Include(o => o.Restaurant)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            // Przekazujemy dane do widoku: dostępne metody dostawy, płatności, statusy
            ViewBag.DeliveryMethods = Enum.GetValues(typeof(DeliveryMethod)).Cast<DeliveryMethod>().ToList();
            ViewBag.PaymentMethods = Enum.GetValues(typeof(PaymentMethod)).Cast<PaymentMethod>().ToList();
            ViewBag.OrderStatuses = Enum.GetValues(typeof(OrderStatus)).Cast<OrderStatus>().ToList();

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,DeliveryMethod,PaymentMethod,PaymentComplete,TimeOrdered,TimePrepared,TimeDelivered,Status,RestaurantId")] Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }

            // Sprawdź, czy restaurantId jest poprawne
            var restaurantExists = await _context.Restaurants.AnyAsync(r => r.Id == order.RestaurantId);
            if (!restaurantExists)
            {
                ModelState.AddModelError("RestaurantId", "Invalid Restaurant ID.");
                ViewBag.DeliveryMethods = Enum.GetValues(typeof(DeliveryMethod)).Cast<DeliveryMethod>().ToList();
                ViewBag.PaymentMethods = Enum.GetValues(typeof(PaymentMethod)).Cast<PaymentMethod>().ToList();
                ViewBag.OrderStatuses = Enum.GetValues(typeof(OrderStatus)).Cast<OrderStatus>().ToList();
                return View(order);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Orders.Any(o => o.Id == order.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Orders", "Restaurants", new { restaurantId = order.RestaurantId });
            }

            // Ponownie załaduj dostępne opcje w przypadku błędu
            ViewBag.DeliveryMethods = Enum.GetValues(typeof(DeliveryMethod)).Cast<DeliveryMethod>().ToList();
            ViewBag.PaymentMethods = Enum.GetValues(typeof(PaymentMethod)).Cast<PaymentMethod>().ToList();
            ViewBag.OrderStatuses = Enum.GetValues(typeof(OrderStatus)).Cast<OrderStatus>().ToList();

            return View(order);
        }
        
        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Restaurant)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Orders == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Orders'  is null.");
            }
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
          return (_context.Orders?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        
        public IActionResult MyOrders()
        {
            var userName = HttpContext.Session.GetInt32("LoggedInUserId");
            var user = _context.Users.FirstOrDefault(u => u.Id == userName);

            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var orders = _context.Orders
                .Where(o => o.UserId == user.Id)
                .ToList();

            return View(orders);
        }
    }
}
