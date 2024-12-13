using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RS.Models;

namespace RS.Controllers
{
    public class OrderServicesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderServicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: OrderServices
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.OrderServices.Include(o => o.Executor).Include(o => o.Order).Include(o => o.Service);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: OrderServices/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderService = await _context.OrderServices
                .Include(o => o.Executor)
                .Include(o => o.Order)
                .Include(o => o.Service)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (orderService == null)
            {
                return NotFound();
            }

            return View(orderService);
        }

        // GET: OrderServices/Create
        public IActionResult Create()
        {
            ViewData["ExecutorId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "CustomerName");
            ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "Name");
            return View();
        }

        // POST: OrderServices/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,OrderId,ServiceId,ExecutorId,AdditionalInfo,CreatedAt,UpdatedAt")] OrderService orderService)
        {
            if (ModelState.IsValid)
            {
                _context.Add(orderService);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ExecutorId"] = new SelectList(_context.Users, "Id", "Id", orderService.ExecutorId);
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "CustomerName", orderService.OrderId);
            ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "Name", orderService.ServiceId);
            return View(orderService);
        }

        // GET: OrderServices/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderService = await _context.OrderServices.FindAsync(id);
            if (orderService == null)
            {
                return NotFound();
            }
            ViewData["ExecutorId"] = new SelectList(_context.Users, "Id", "Id", orderService.ExecutorId);
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "CustomerName", orderService.OrderId);
            ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "Name", orderService.ServiceId);
            return View(orderService);
        }

        // POST: OrderServices/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,OrderId,ServiceId,ExecutorId,AdditionalInfo,CreatedAt,UpdatedAt")] OrderService orderService)
        {
            if (id != orderService.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(orderService);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderServiceExists(orderService.Id))
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
            ViewData["ExecutorId"] = new SelectList(_context.Users, "Id", "Id", orderService.ExecutorId);
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "CustomerName", orderService.OrderId);
            ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "Name", orderService.ServiceId);
            return View(orderService);
        }

        // GET: OrderServices/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderService = await _context.OrderServices
                .Include(o => o.Executor)
                .Include(o => o.Order)
                .Include(o => o.Service)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (orderService == null)
            {
                return NotFound();
            }

            return View(orderService);
        }

        // POST: OrderServices/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var orderService = await _context.OrderServices.FindAsync(id);
            if (orderService != null)
            {
                _context.OrderServices.Remove(orderService);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderServiceExists(int id)
        {
            return _context.OrderServices.Any(e => e.Id == id);
        }
    }
}
