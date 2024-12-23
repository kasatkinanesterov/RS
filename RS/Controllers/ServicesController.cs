using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RS.Models;
using RS.ViewModels.Services;
using System.IO;

namespace RS.Controllers
{
    public class ServicesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ServicesController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ServicesController(ApplicationDbContext context, ILogger<ServicesController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        [Authorize]
        public IActionResult Index()
        {
            var services = _context.Services
                .Include(s => s.Role)
                .ToList();

            var model = services.Select(s => new ServiceViewModel
            {
                Id = s.Id,
                Name = s.Name,
                Price = s.Price,
                Description = s.Description,
                Photo = s.Photo,
                RoleName = s.Role?.Name,
                IsActive = s.IsActive
            });

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.Roles = new SelectList(_context.Roles.ToList(), "Id", "Name");
            return PartialView("~/Views/Services/Partials/CreateServicePartial.cshtml", new ServiceViewModel());
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceViewModel model, IFormFile? photoFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new SelectList(_context.Roles.ToList(), "Id", "Name");
                return PartialView("~/Views/Services/Partials/CreateServicePartial.cshtml", model);
            }

            string? photoPath = null;
            if (photoFile != null)
            {
                photoPath = await SavePhotoAsync(photoFile);
            }

            var service = new Service
            {
                Name = model.Name,
                Price = model.Price,
                Description = model.Description,
                Photo = photoPath,
                RoleId = model.RoleId,
                IsActive = model.IsActive
            };

            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var service = await _context.Services
                .Include(s => s.Role)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (service == null) return NotFound();

            ViewBag.ServiceName = service.Name; // Название для отображения
            ViewBag.RoleName = service.Role?.Name; // Название роли для отображения

            var model = new EditServiceViewModel
            {
                Id = service.Id,
                Price = service.Price,
                Description = service.Description,
                ExistingPhoto = service.Photo,
                RoleId = service.RoleId, // Передаем RoleId для скрытого поля
                IsActive = service.IsActive
            };

            return PartialView("~/Views/Services/Partials/EditServicePartial.cshtml", model);
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditServiceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("~/Views/Services/Partials/EditServicePartial.cshtml", model);
            }

            var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == model.Id);
            if (service == null) return NotFound();

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

            service.Price = model.Price;
            service.Description = model.Description;
            service.Photo = photoPath;
            service.IsActive = model.IsActive;

            _context.Update(service);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var service = await _context.Services.Include(s => s.Role).FirstOrDefaultAsync(s => s.Id == id);
            if (service == null) return NotFound();

            var model = new ServiceViewModel
            {
                Id = service.Id,
                Name = service.Name,
                Price = service.Price,
                Description = service.Description,
                Photo = service.Photo,
                RoleName = service.Role?.Name,
                IsActive = service.IsActive
            };

            return PartialView("~/Views/Services/Partials/DetailsServicePartial.cshtml", model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ConfirmDelete(int id)
        {
            var service = await _context.Services.Include(s => s.Role).FirstOrDefaultAsync(s => s.Id == id);
            if (service == null) return NotFound();

            var model = new ServiceViewModel
            {
                Id = service.Id,
                Name = service.Name,
                Price = service.Price,
                Description = service.Description,
                Photo = service.Photo,
                RoleName = service.Role?.Name
            };

            return PartialView("~/Views/Services/Partials/ConfirmDeleteServicePartial.cshtml", model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == id);
            if (service == null) return Json(new { success = false, message = "Услуга не найдена." });

            try
            {
                if (!string.IsNullOrEmpty(service.Photo))
                {
                    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, service.Photo.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка при удалении услуги: {Message}", ex.Message);
                return Json(new { success = false, message = "Ошибка при удалении услуги." });
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
                throw;
            }
        }
    }
}
