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
using RS.ViewModels.OrderProducts;
using RS.ViewModels.Products;

namespace RS.Controllers
{
    public class OrderProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrdersController> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        public OrderProductsController(ApplicationDbContext context, ILogger<OrdersController> logger, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize(Roles = "Agent,Admin")]
        public IActionResult SelectProduct(int orderId)
        {
            var products = _context.Products.ToList();
            ViewBag.Products = new SelectList(products, "Id", "Name");

            var model = new SelectProductViewModel { OrderId = orderId };
            return PartialView("~/Views/OrderProducts/Partials/SelectProductPartial.cshtml", model);
        }

        [HttpPost]
        [Authorize(Roles = "Agent,Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult SelectProduct(SelectProductViewModel model)
        {
            // Проверяем, был ли выбран товар
            if (!model.ProductId.HasValue)
            {
                ModelState.AddModelError("", "Вы должны выбрать товар.");

                // Заново загружаем список товаров
                var products = _context.Products.ToList();
                ViewBag.Products = new SelectList(products, "Id", "Name");

                // Возвращаем частичное представление для повторного выбора
                return PartialView("~/Views/OrderProducts/Partials/SelectProductPartial.cshtml", model);
            }

            // Получаем товар по выбранному ProductId
            var product = _context.Products.FirstOrDefault(p => p.Id == model.ProductId.Value);
            if (product == null)
            {
                ModelState.AddModelError("", "Выбранный товар не найден.");

                // Заново загружаем список товаров
                var products = _context.Products.ToList();
                ViewBag.Products = new SelectList(products, "Id", "Name");

                // Возвращаем частичное представление для повторного выбора
                return PartialView("~/Views/OrderProducts/Partials/SelectProductPartial.cshtml", model);
            }

            // Получаем список исполнителей для выбранного товара
            var executors = _userManager.Users
                .Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == product.RoleId))
                .Select(u => new { u.Id, u.UserName })
                .ToList();

            // Передаем список исполнителей в ViewBag
            ViewBag.Executors = new SelectList(executors, "Id", "UserName");

            // Формируем модель для следующего этапа
            var editModel = new OrderProductEditViewModel
            {
                OrderId = model.OrderId,
                ProductId = model.ProductId.Value
            };

            // Возвращаем частичное представление для заполнения данных
            return PartialView("~/Views/OrderProducts/Partials/AddOrderProductPartial.cshtml", editModel);
        }

        // POST: Orders/AddProductToOrder
        [HttpPost]
        [Authorize(Roles = "Agent,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrderProductDetails(OrderProductEditViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == model.OrderId);
            if (order == null || (User.IsInRole("Agent") && order.EmployeeId != user.Id))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                var product = await _context.Products.FindAsync(model.ProductId);
                var executors = _userManager.Users
                    .Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == product.RoleId))
                    .Select(u => new { u.Id, u.UserName })
                    .ToList();
                ViewBag.Executors = new SelectList(executors, "Id", "UserName");

                return PartialView("~/Views/OrderProducts/Partials/AddOrderProductPartial.cshtml", model);
            }

            var orderProduct = new OrderProduct
            {
                OrderId = model.OrderId,
                ProductId = model.ProductId,
                Status = model.Status,
                AdditionalInfo = model.AdditionalInfo,
                ExecutorId = model.ExecutorId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Deadline = model.Deadline
            };

            _context.OrderProducts.Add(orderProduct);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }





        // GET: Orders/EditOrderProduct
        [HttpGet]
        [Authorize(Roles = "Agent,Admin")]
        public async Task<IActionResult> EditOrderProduct(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var orderProduct = await _context.OrderProducts
                .Include(op => op.Order)
                .Include(op => op.Product)
                .FirstOrDefaultAsync(op => op.Id == id);

            if (orderProduct == null) return NotFound();

            // Проверка прав для агента
            if (User.IsInRole("Agent") && orderProduct.Order.EmployeeId != user.Id)
            {
                _logger.LogWarning("EditOrderProduct: Агент {UserId} попытался редактировать товар в чужом заказе {OrderId}", user.Id, orderProduct.OrderId);
                return Forbid();
            }

            // Получаем исполнителей с подходящей ролью
            var roleId = orderProduct.Product.RoleId;
            var executors = _userManager.Users
                .Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == roleId))
                .Select(u => new { u.Id, u.UserName })
                .ToList();

            ViewBag.Executors = new SelectList(executors, "Id", "UserName", orderProduct.ExecutorId);
            ViewBag.ProductName = orderProduct.Product.Name;

            return PartialView("~/Views/OrderProducts/Partials/EditOrderProductPartial.cshtml", new OrderProductEditViewModel
            {
                Id = orderProduct.Id,
                OrderId = orderProduct.OrderId,
                ProductId = orderProduct.ProductId, // Поле будет недоступным
                Status = orderProduct.Status,
                ExecutorId = orderProduct.ExecutorId,
                AdditionalInfo = orderProduct.AdditionalInfo,
                Deadline = orderProduct.Deadline
            });
        }


        // POST: Orders/EditOrderProduct
        [HttpPost]
        [Authorize(Roles = "Agent,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOrderProduct(OrderProductEditViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            var orderProduct = await _context.OrderProducts
                .Include(op => op.Order)
                .Include(op => op.Product)
                .FirstOrDefaultAsync(op => op.Id == model.Id);

            if (orderProduct == null) return NotFound();

            // Проверка прав для агента
            if (User.IsInRole("Agent") && orderProduct.Order.EmployeeId != user.Id)
            {
                _logger.LogWarning("EditOrderProduct: Агент {UserId} попытался редактировать товар в чужом заказе {OrderId}.", user.Id, orderProduct.OrderId);
                return Forbid();
            }

            // Проверяем роль исполнителя
            var roleId = orderProduct.Product.RoleId;
            var executorHasRole = _context.UserRoles.Any(ur => ur.UserId == model.ExecutorId && ur.RoleId == roleId);
            if (!executorHasRole)
            {
                ModelState.AddModelError("", "Выбранный исполнитель не имеет требуемой роли для данного товара.");
                return Json(new { success = false, message = "Неверная роль исполнителя." });
            }

            if (!ModelState.IsValid)
            {
                return PartialView("~/Views/OrderProducts/Partials/EditOrderProductPartial.cshtml", model);
            }

            try
            {
                // Обновляем только разрешённые поля
                orderProduct.ExecutorId = model.ExecutorId;
                orderProduct.Status = model.Status;
                orderProduct.AdditionalInfo = model.AdditionalInfo;
                orderProduct.Deadline = model.Deadline;
                orderProduct.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError("EditOrderProduct: Ошибка при редактировании. Exception: {Message}", ex.Message);
                return Json(new { success = false, message = "Ошибка при редактировании." });
            }
        }


        // GET: Orders/EditOrderProductByExecutor
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditOrderProductByExecutor(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            _logger.LogInformation("EditOrderProductByExecutor: User {UserId} is attempting to load product {ProductId}", user.Id, id);

            var orderProduct = await _context.OrderProducts
                .Include(op => op.Executor)
                .FirstOrDefaultAsync(op => op.Id == id && op.ExecutorId == user.Id);

            if (orderProduct == null)
            {
                _logger.LogWarning("EditOrderProductByExecutor: Access denied. User {UserId} does not have access to product {ProductId}", user.Id, id);
                return Forbid(); // Вернуть ошибку доступа
            }

            var model = new OrderProductExecutorEditViewModel
            {
                Id = orderProduct.Id,
                Status = orderProduct.Status,
                AdditionalInfo = orderProduct.AdditionalInfo
            };

            _logger.LogInformation("EditOrderProductByExecutor: Form successfully loaded for User {UserId}, Product {ProductId}", user.Id, id);

            return PartialView("~/Views/OrderProducts/Partials/EditOrderProductByExecutorPartial.cshtml", model);
        }


        // POST: Orders/EditOrderProductByExecutor
        [HttpPost]
        [Authorize] //Добавить будущие роли
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOrderProductByExecutor(OrderProductExecutorEditViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            // Проверка товара и доступа
            var orderProduct = await _context.OrderProducts
                .Include(op => op.Executor)
                .FirstOrDefaultAsync(op => op.Id == model.Id && op.ExecutorId == user.Id);

            if (orderProduct == null)
            {
                _logger.LogWarning("EditOrderProductByExecutor: Исполнитель {UserId} не имеет доступа к товару {ProductId}", user.Id, model.Id);
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("EditOrderProductByExecutor: Валидация не пройдена.");
                return PartialView("~/Views/OrderProducts/Partials/EditOrderProductByExecutorPartial.cshtml", model);
            }

            // Обновляем разрешённые поля
            orderProduct.Status = model.Status;
            orderProduct.AdditionalInfo = model.AdditionalInfo;
            orderProduct.UpdatedAt = DateTime.Now; // Обновляем дату

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("EditOrderProductByExecutor: Товар {ProductId} успешно обновлён исполнителем {UserId}", orderProduct.Id, user.Id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError("EditOrderProductByExecutor: Ошибка при обновлении товара {ProductId}. Exception: {Message}", orderProduct.Id, ex.Message);
                return Json(new { success = false, message = "Ошибка при обновлении товара." });
            }
        }

        //GET Delete
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ConfirmDeleteOrderProduct(int id)
        {
            var orderProduct = await _context.OrderProducts
                .Include(op => op.Product)
                .Include(op => op.Executor) // Исполнитель
                .FirstOrDefaultAsync(op => op.Id == id);

            if (orderProduct == null)
            {
                return NotFound("Товар не найден.");
            }

            // Возвращаем частичное представление с полной информацией о товаре
            return PartialView("~/Views/OrderProducts/Partials/ConfirmDeleteOrderProductPartial.cshtml", new OrderProductViewModel
            {
                Id = orderProduct.Id,
                ProductName = orderProduct.Product.Name,
                ProductPrice = orderProduct.Product.Price,
                Status = orderProduct.Status,
                AdditionalInfo = orderProduct.AdditionalInfo,
                CreatedAt = orderProduct.CreatedAt,
                UpdatedAt = orderProduct.UpdatedAt,
                Deadline = orderProduct.Deadline,
                ExecutorName = orderProduct.Executor?.UserName ?? "Не назначен"
            });
        }


        // POST: Orders/DeleteProductFromOrder
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOrderProduct(int id)
        {
            var orderProduct = await _context.OrderProducts.FindAsync(id);
            if (orderProduct == null)
            {
                _logger.LogWarning("DeleteOrderProduct: Товар с ID {ProductId} не найден.", id);
                return Json(new { success = false, message = "Товар не найден." });
            }

            try
            {
                _context.OrderProducts.Remove(orderProduct);
                await _context.SaveChangesAsync();
                _logger.LogInformation("DeleteOrderProduct: Товар с ID {ProductId} успешно удалён.", id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError("DeleteOrderProduct: Ошибка при удалении товара {ProductId}. Exception: {Message}", id, ex.Message);
                return Json(new { success = false, message = "Ошибка при удалении товара." });
            }
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Role)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            var model = new ProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Description = product.Description,
                ExistingPhoto = product.Photo,
                RoleName = product.Role?.Name
            };

            return PartialView("~/Views/Products/Partials/DetailsProductPartial.cshtml", model);
        }

        private bool OrderProductExists(int id)
        {
            return _context.OrderProducts.Any(e => e.Id == id);
        }
    }
}
