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
    public class UserMessagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserMessagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: UserMessages
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.UsersMessages.Include(u => u.Message).Include(u => u.Restaurant);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: UserMessages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.UsersMessages == null)
            {
                return NotFound();
            }

            var userMessage = await _context.UsersMessages
                .Include(u => u.Message)
                .Include(u => u.Restaurant)
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (userMessage == null)
            {
                return NotFound();
            }

            return View(userMessage);
        }

        // GET: UserMessages/Create
        public IActionResult Create()
        {
            ViewData["MessageId"] = new SelectList(_context.Messages, "Id", "Content");
            ViewData["RestaurantId"] = new SelectList(_context.Restaurants, "Id", "Name");
            return View();
        }

        // POST: UserMessages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,MessageId,RestaurantId")] UserMessage userMessage)
        {
            if (ModelState.IsValid)
            {
                _context.Add(userMessage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MessageId"] = new SelectList(_context.Messages, "Id", "Content", userMessage.MessageId);
            ViewData["RestaurantId"] = new SelectList(_context.Restaurants, "Id", "Name", userMessage.RestaurantId);
            return View(userMessage);
        }

        // GET: UserMessages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.UsersMessages == null)
            {
                return NotFound();
            }

            var userMessage = await _context.UsersMessages.FindAsync(id);
            if (userMessage == null)
            {
                return NotFound();
            }
            ViewData["MessageId"] = new SelectList(_context.Messages, "Id", "Content", userMessage.MessageId);
            ViewData["RestaurantId"] = new SelectList(_context.Restaurants, "Id", "Name", userMessage.RestaurantId);
            return View(userMessage);
        }

        // POST: UserMessages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,MessageId,RestaurantId")] UserMessage userMessage)
        {
            if (id != userMessage.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userMessage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserMessageExists(userMessage.UserId))
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
            ViewData["MessageId"] = new SelectList(_context.Messages, "Id", "Content", userMessage.MessageId);
            ViewData["RestaurantId"] = new SelectList(_context.Restaurants, "Id", "Name", userMessage.RestaurantId);
            return View(userMessage);
        }

        // GET: UserMessages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.UsersMessages == null)
            {
                return NotFound();
            }

            var userMessage = await _context.UsersMessages
                .Include(u => u.Message)
                .Include(u => u.Restaurant)
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (userMessage == null)
            {
                return NotFound();
            }

            return View(userMessage);
        }

        // POST: UserMessages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.UsersMessages == null)
            {
                return Problem("Entity set 'ApplicationDbContext.UsersMessages'  is null.");
            }
            var userMessage = await _context.UsersMessages.FindAsync(id);
            if (userMessage != null)
            {
                _context.UsersMessages.Remove(userMessage);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserMessageExists(int id)
        {
          return (_context.UsersMessages?.Any(e => e.UserId == id)).GetValueOrDefault();
        }
    }
}
