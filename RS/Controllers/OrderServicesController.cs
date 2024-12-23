using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RS.Models;
using RS.ViewModels.OrderServices;

namespace RS.Controllers
{
    public class OrderServicesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrdersController> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        public OrderServicesController(ApplicationDbContext context, ILogger<OrdersController> logger, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize(Roles = "Agent,Admin")]
        public IActionResult SelectService(int orderId)
        {
            var services = _context.Services.ToList();
            ViewBag.Services = new SelectList(services, "Id", "Name");

            var model = new SelectServiceViewModel { OrderId = orderId };
            return PartialView("~/Views/OrderServices/Partials/SelectServicePartial.cshtml", model);
        }

        [HttpPost]
        [Authorize(Roles = "Agent,Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult SelectService(SelectServiceViewModel model)
        {
            if (!model.ServiceId.HasValue)
            {
                ModelState.AddModelError("", "Вы должны выбрать услугу.");

                var services = _context.Services.ToList();
                ViewBag.Services = new SelectList(services, "Id", "Name");

                return PartialView("~/Views/OrderServices/Partials/SelectServicePartial.cshtml", model);
            }

            var service = _context.Services.FirstOrDefault(s => s.Id == model.ServiceId.Value);
            if (service == null)
            {
                ModelState.AddModelError("", "Выбранная услуга не найдена.");

                var services = _context.Services.ToList();
                ViewBag.Services = new SelectList(services, "Id", "Name");

                return PartialView("~/Views/OrderServices/Partials/SelectServicePartial.cshtml", model);
            }

            var executors = _userManager.Users
                .Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == service.RoleId))
                .Select(u => new { u.Id, u.UserName })
                .ToList();

            ViewBag.Executors = new SelectList(executors, "Id", "UserName");

            var editModel = new OrderServiceEditViewModel
            {
                OrderId = model.OrderId,
                ServiceId = model.ServiceId.Value
            };

            return PartialView("~/Views/OrderServices/Partials/AddOrderServicePartial.cshtml", editModel);
        }


        // POST: Orders/AddServiceToOrder
        [HttpPost]
        [Authorize(Roles = "Agent,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrderServiceDetails(OrderServiceEditViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == model.OrderId);
            if (order == null || (User.IsInRole("Agent") && order.EmployeeId != user.Id))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                var service = await _context.Services.FindAsync(model.ServiceId);
                var executors = _userManager.Users
                    .Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == service.RoleId))
                    .Select(u => new { u.Id, u.UserName })
                    .ToList();

                ViewBag.Executors = new SelectList(executors, "Id", "UserName");

                return PartialView("~/Views/OrderServices/Partials/AddOrderServicePartial.cshtml", model);
            }

            var orderService = new OrderService
            {
                OrderId = model.OrderId,
                ServiceId = model.ServiceId,
                ExecutorId = model.ExecutorId,
                Status = model.Status,
                AdditionalInfo = model.AdditionalInfo,
                Deadline = model.Deadline,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.OrderServices.Add(orderService);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }




        // GET: Orders/EditOrderService
        [HttpGet]
        [Authorize(Roles = "Agent,Admin")]
        public async Task<IActionResult> EditOrderService(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var orderService = await _context.OrderServices
                .Include(os => os.Order)
                .Include(os => os.Service)
                .FirstOrDefaultAsync(os => os.Id == id);

            if (orderService == null) return NotFound();

            if (User.IsInRole("Agent") && orderService.Order.EmployeeId != user.Id)
            {
                _logger.LogWarning("EditOrderService: Агент {UserId} попытался редактировать услугу в чужом заказе {OrderId}.", user.Id, orderService.OrderId);
                return Forbid();
            }

            var executors = _userManager.Users
                .Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == orderService.Service.RoleId))
                .Select(u => new { u.Id, u.UserName })
                .ToList();

            ViewBag.Executors = new SelectList(executors, "Id", "UserName", orderService.ExecutorId);
            ViewBag.ServiceName = orderService.Service.Name;
            return PartialView("~/Views/OrderServices/Partials/EditOrderServicePartial.cshtml", new OrderServiceEditViewModel
            {
                Id = orderService.Id,
                OrderId = orderService.OrderId,
                ServiceId = orderService.ServiceId, // Поле будет недоступным
                Status = orderService.Status,
                ExecutorId = orderService.ExecutorId,
                AdditionalInfo = orderService.AdditionalInfo,
                Deadline = orderService.Deadline
            });
        }


        // POST: Orders/EditOrderService
        [HttpPost]
        [Authorize(Roles = "Agent,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOrderService(OrderServiceEditViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            var orderService = await _context.OrderServices
                .Include(os => os.Order)
                .Include(os => os.Service)
                .FirstOrDefaultAsync(os => os.Id == model.Id);

            if (orderService == null) return NotFound();

            if (User.IsInRole("Agent") && orderService.Order.EmployeeId != user.Id)
            {
                _logger.LogWarning("EditOrderService: Агент {UserId} попытался редактировать услугу в чужом заказе {OrderId}.", user.Id, orderService.OrderId);
                return Forbid();
            }

            var executorHasRole = _context.UserRoles.Any(ur => ur.UserId == model.ExecutorId && ur.RoleId == orderService.Service.RoleId);
            if (!executorHasRole)
            {
                ModelState.AddModelError("", "Выбранный исполнитель не имеет необходимой роли для данной услуги.");
                return Json(new { success = false, message = "Неверная роль исполнителя." });
            }

            if (!ModelState.IsValid)
            {
                return PartialView("~/Views/OrderServices/Partials/EditOrderServicePartial.cshtml", model);
            }

            try
            {
                // Обновляем только разрешённые поля
                orderService.ExecutorId = model.ExecutorId;
                orderService.Status = model.Status;
                orderService.AdditionalInfo = model.AdditionalInfo;
                orderService.Deadline = model.Deadline;
                orderService.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError("EditOrderService: Ошибка при редактировании. Exception: {Message}", ex.Message);
                return Json(new { success = false, message = "Ошибка при редактировании." });
            }
        }


        // GET: Orders/EditOrderServiceByExecutor
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditOrderServiceByExecutor(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var orderService = await _context.OrderServices
                .FirstOrDefaultAsync(os => os.Id == id && os.ExecutorId == user.Id);

            if (orderService == null) return Forbid();

            return PartialView("~/Views/OrderServices/Partials/EditOrderServiceByExecutorPartial.cshtml", new OrderServiceExecutorEditViewModel
            {
                Id = orderService.Id,
                Status = orderService.Status,
                AdditionalInfo = orderService.AdditionalInfo
            });
        }

        // POST: Orders/EditOrderServiceByExecutor
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOrderServiceByExecutor(OrderServiceExecutorEditViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            var orderService = await _context.OrderServices
                .FirstOrDefaultAsync(os => os.Id == model.Id && os.ExecutorId == user.Id);

            if (orderService == null) return Forbid();

            orderService.Status = model.Status;
            orderService.AdditionalInfo = model.AdditionalInfo;
            orderService.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // GET: Orders/ConfirmDeleteOrderService
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ConfirmDeleteOrderService(int id)
        {
            var orderService = await _context.OrderServices
                .Include(os => os.Service)
                .Include(os => os.Executor)
                .FirstOrDefaultAsync(os => os.Id == id);

            if (orderService == null) return NotFound();

            return PartialView("~/Views/OrderServices/Partials/ConfirmDeleteOrderServicePartial.cshtml", new OrderServiceViewModel
            {
                Id = orderService.Id,
                ServiceName = orderService.Service.Name,
                ServicePrice = orderService.Service.Price,
                ExecutorName = orderService.Executor?.UserName ?? "Не назначен",
                Status = orderService.Status,
                AdditionalInfo = orderService.AdditionalInfo,
                CreatedAt = orderService.CreatedAt,
                UpdatedAt = orderService.UpdatedAt,
                Deadline = orderService.Deadline
            });
        }

        // POST: Orders/DeleteServiceFromOrder
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOrderService(int id)
        {
            var orderService = await _context.OrderServices.FindAsync(id);
            if (orderService == null) return Json(new { success = false, message = "Услуга не найдена." });

            _context.OrderServices.Remove(orderService);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        private bool OrderServiceExists(int id)
        {
            return _context.OrderServices.Any(e => e.Id == id);
        }
    }
}
