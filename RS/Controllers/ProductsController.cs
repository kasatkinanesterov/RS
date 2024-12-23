using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RS.Models;
using RS.ViewModels.Products;
using System.IO;

namespace RS.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductsController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductsController(ApplicationDbContext context, ILogger<ProductsController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        [Authorize]
        public IActionResult Index()
        {
            var products = _context.Products
                .Include(p => p.Role)
                .ToList();

            var model = products.Select(p => new ProductViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Description = p.Description,
                Photo = p.Photo,
                RoleName = p.Role?.Name
            });

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.Roles = new SelectList(_context.Roles.ToList(), "Id", "Name");
            return PartialView("~/Views/Products/Partials/CreateProductPartial.cshtml", new ProductViewModel());
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel model, IFormFile? photoFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new SelectList(_context.Roles.ToList(), "Id", "Name");
                return PartialView("~/Views/Products/Partials/CreateProductPartial.cshtml", model);
            }

            string? photoPath = null;
            if (photoFile != null)
            {
                photoPath = await SavePhotoAsync(photoFile);
            }

            var product = new Product
            {
                Name = model.Name,
                Price = model.Price,
                Description = model.Description,
                Photo = photoPath, // Записываем путь к фото
                RoleId = model.RoleId
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }


        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products
                .Include(p => p.Role)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            ViewBag.ProductName = product.Name; // Название для отображения
            ViewBag.RoleName = product.Role?.Name; // Название роли для отображения

            var model = new EditProductViewModel
            {
                Id = product.Id,
                Price = product.Price,
                Description = product.Description,
                ExistingPhoto = product.Photo,
                RoleId = product.RoleId // Передаём RoleId для скрытого поля
            };

            return PartialView("~/Views/Products/Partials/EditProductPartial.cshtml", model);
        }





        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("~/Views/Products/Partials/EditProductPartial.cshtml", model);
            }

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == model.Id);
            if (product == null) return NotFound();

            string? photoPath = model.ExistingPhoto;
            if (model.PhotoFile != null)
            {
                photoPath = await SavePhotoAsync(model.PhotoFile);

                if (!string.IsNullOrEmpty(model.ExistingPhoto))
                {
                    var existingFilePath = Path.Combine(_webHostEnvironment.WebRootPath, model.ExistingPhoto.TrimStart('/'));
                    if (System.IO.File.Exists(existingFilePath))
                    {
                        System.IO.File.Delete(existingFilePath);
                    }
                }
            }

            product.Price = model.Price;
            product.Description = model.Description;
            product.Photo = photoPath;

            _context.Update(product);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }




        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products.Include(p => p.Role).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();

            var model = new ProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Description = product.Description,
                Photo = product.Photo, // Путь к фото
                RoleName = product.Role?.Name
            };

            return PartialView("~/Views/Products/Partials/DetailsProductPartial.cshtml", model);
        }


        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ConfirmDelete(int id)
        {
            var product = await _context.Products.Include(p => p.Role).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();

            var model = new ProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Description = product.Description,
                Photo = product.Photo,
                RoleName = product.Role?.Name
            };

            return PartialView("~/Views/Products/Partials/ConfirmDeleteProductPartial.cshtml", model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return Json(new { success = false, message = "Товар не найден." });

            try
            {
                // Удаление файла фото
                if (!string.IsNullOrEmpty(product.Photo))
                {
                    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, product.Photo.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка при удалении товара: {Message}", ex.Message);
                return Json(new { success = false, message = "Ошибка при удалении товара." });
            }
        }

        private async Task<string?> SavePhotoAsync(IFormFile? photo)
        {
            if (photo == null || photo.Length == 0)
            {
                _logger.LogWarning("Файл не был предоставлен или его длина равна 0.");
                return null;
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(photo.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                _logger.LogError("Недопустимый формат файла: {FileExtension}.", fileExtension);
                throw new InvalidOperationException("Недопустимый формат файла.");
            }

            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{photo.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            try
            {
                await using var fileStream = new FileStream(filePath, FileMode.Create);
                await photo.CopyToAsync(fileStream);

                _logger.LogInformation("Файл сохранен успешно: {FilePath}", filePath);
                return $"/uploads/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении файла.");
                throw; // Пробрасываем ошибку дальше
            }
        }
    }
}
