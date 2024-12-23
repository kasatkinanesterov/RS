using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RS.Models;
using System;
using System.CodeDom.Compiler;
using System.Linq;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;
using RS.ViewModels.Orders;
using RS.ViewModels.OrderProducts;
using RS.ViewModels.OrderServices;

namespace RS.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrdersController> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        public OrdersController(ApplicationDbContext context, ILogger<OrdersController> logger, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        // GET: Orders/Create
        [HttpGet]
        [Authorize(Roles = "Agent,Admin")]
        public IActionResult Create()
        {
            var user = _userManager.GetUserAsync(User).Result;

            var model = new CreateOrderViewModel
            {
                EmployeeId = user?.Id,
                Employees = _context.Users
                    .Select(u => new SelectListItem { Value = u.Id, Text = u.UserName })
                    .ToList()
            };

            return View(model);
        }



        // POST: Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Agent,Admin")]
        public async Task<IActionResult> Create(CreateOrderViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid)
            {
                model.Employees = _context.Users
                    .Select(u => new SelectListItem { Value = u.Id, Text = u.UserName })
                    .ToList();
                return View(model);
            }

            // Принудительно устанавливаем EmployeeId на текущего пользователя
            model.EmployeeId = user?.Id;

            var order = new Order
            {
                CustomerName = model.CustomerName,
                CustomerEmail = model.CustomerEmail,
                CustomerPhone = model.CustomerPhone,
                OrderStatus = model.OrderStatus,
                EmployeeId = model.EmployeeId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            try
            {
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при создании заказа: {ex.Message}");
                ModelState.AddModelError("", "Произошла ошибка при создании заказа.");
                return View(model);
            }
        }


        [Authorize]
        // GET: Orders/Index
        public async Task<IActionResult> Index()
        {
            var orders = _context.Orders.Include(o => o.Employee).ToList();
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);

            ViewBag.UserRole = roles.FirstOrDefault(); // Получаем роль текущего пользователя

            return View(orders);
        }


        // GET: Orders/EditOrderByAdmin/5
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditOrderByAdmin(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            var model = new AdminEditOrderViewModel
            {
                Id = order.Id,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                CustomerPhone = order.CustomerPhone,
                OrderStatus = order.OrderStatus,
                EmployeeId = order.EmployeeId
            };

            ViewBag.Users = new SelectList(_context.Users, "Id", "UserName", order.EmployeeId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("~/Views/Orders/Partials/EditOrderByAdminPartial.cshtml", model);
            }

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOrderByAdmin(AdminEditOrderViewModel model)
        {
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("~/Views/Orders/Partials/EditOrderByAdminPartial.cshtml", model);
                }
                return View(model);
            }

            var order = await _context.Orders.FindAsync(model.Id);
            if (order == null) return NotFound();

            // Обновляем все поля
            order.CustomerName = model.CustomerName;
            order.CustomerEmail = model.CustomerEmail;
            order.CustomerPhone = model.CustomerPhone;
            order.OrderStatus = model.OrderStatus;
            order.EmployeeId = model.EmployeeId;
            order.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Orders/EditOrderByAgent/5
        [HttpGet]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> EditOrderByAgent(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id && o.EmployeeId == user.Id);
            if (order == null) return Forbid();

            var model = new AgentEditOrderViewModel
            {
                Id = order.Id,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                CustomerPhone = order.CustomerPhone,
                OrderStatus = order.OrderStatus
            };

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("~/Views/Orders/Partials/EditOrderByAgentPartial.cshtml", model);
            }

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Agent")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOrderByAgent(AgentEditOrderViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == model.Id && o.EmployeeId == user.Id);

            if (order == null) return Forbid();

            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("~/Views/Orders/Partials/EditOrderByAgentPartial.cshtml", model);
                }
                return View(model);
            }

            // Обновляем разрешенные поля
            order.CustomerName = model.CustomerName;
            order.CustomerEmail = model.CustomerEmail;
            order.CustomerPhone = model.CustomerPhone;
            order.OrderStatus = model.OrderStatus;
            order.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Employee)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);

            ViewBag.CanEditAsAdmin = roles.Contains("Admin");
            ViewBag.CanEditAsAgent = roles.Contains("Agent") && order.EmployeeId == user.Id;
            ViewBag.CurrentUserId = _userManager.GetUserId(User);

            // Получение продуктов и услуг через отдельные методы
            var orderProducts = await GetProductsForOrder(id);
            var orderServices = await GetServicesForOrder(id);

            var model = new OrderDetailsViewModel
            {
                OrderId = order.Id,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                CustomerPhone = order.CustomerPhone,
                OrderStatus = order.OrderStatus,
                EmployeeName = order.Employee?.UserName ?? "Не назначен",
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                Products = orderProducts,
                Services = orderServices
            };

            return View(model);
        }

        // Получение продуктов для заказа
        private async Task<List<OrderProductViewModel>> GetProductsForOrder(int orderId)
        {
            var orderProducts = await _context.OrderProducts
                .Include(op => op.Product)
                .Include(op => op.Executor)
                .Where(op => op.OrderId == orderId)
                .ToListAsync();

            return orderProducts.Select(op => new OrderProductViewModel
            {
                Id = op.Id,
                ProductName = op.Product.Name,
                ProductPrice = op.Product.Price,
                Status = op.Status,
                ExecutorName = op.Executor?.UserName,
                ExecutorId = op.ExecutorId,
                AdditionalInfo = op.AdditionalInfo,
                CreatedAt = op.CreatedAt,
                UpdatedAt = op.UpdatedAt,
                Deadline = op.Deadline
            }).ToList();
        }

        // Получение услуг для заказа
        private async Task<List<OrderServiceViewModel>> GetServicesForOrder(int orderId)
        {
            var orderServices = await _context.OrderServices
                .Include(os => os.Service)
                .Include(os => os.Executor)
                .Where(os => os.OrderId == orderId)
                .ToListAsync();

            return orderServices.Select(os => new OrderServiceViewModel
            {
                Id = os.Id,
                ServiceName = os.Service.Name,
                ServicePrice = os.Service.Price,
                Status = os.Status,
                ExecutorName = os.Executor?.UserName,
                ExecutorId = os.ExecutorId,
                AdditionalInfo = os.AdditionalInfo,
                CreatedAt = os.CreatedAt,
                UpdatedAt = os.UpdatedAt,
                Deadline = os.Deadline
            }).ToList();
        }

        // GET: Orders/Delete/5
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ConfirmDeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            var model = new ConfirmDeleteOrderViewModel
            {
                OrderId = order.Id,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail, // Добавлено
                OrderStatus = order.OrderStatus
            };

            return PartialView("~/Views/Orders/Partials/ConfirmDeleteOrderPartial.cshtml", model);
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOrderByAdmin(ConfirmDeleteOrderViewModel model)
        {
            var order = await _context.Orders.FindAsync(model.OrderId);
            if (order == null)
            {
                return Json(new { success = false, message = "Заказ не найден." });
            }

            try
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при удалении заказа с ID {model.OrderId}: {ex.Message}");
                return Json(new { success = false, message = "Произошла ошибка при удалении заказа." });
            }
        }



        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}

