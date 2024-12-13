using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RS.Models;

namespace RS.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Отчёт по заказам
        public async Task<IActionResult> OrdersReport()
        {
            var orderProducts = await _context.OrderProducts
                .Include(op => op.Product)
                .ToListAsync();

            var orderServices = await _context.OrderServices
                .Include(os => os.Service)
                .ToListAsync();

            var orders = await _context.Orders
                .Include(o => o.Employee)
                .ToListAsync();

            var report = orders.Select(o => new OrderReportViewModel
            {
                OrderId = o.Id,
                CustomerName = o.CustomerName,
                Status = o.OrderStatus,
                EmployeeName = o.Employee?.FullName ?? "Не назначен",
                Products = orderProducts
                    .Where(op => op.OrderId == o.Id)
                    .Select(op => new ProductReportItem
                    {
                        Name = op.Product.Name,
                        Quantity = op.Quantity
                    })
                    .ToList(),
                Services = orderServices
                    .Where(os => os.OrderId == o.Id)
                    .Select(os => os.Service.Name)
                    .ToList()
            }).ToList();

            return View(report);
        }
        public async Task<IActionResult> PopularityReport()
        {
            // Данные по товарам
            var productPopularity = await _context.OrderProducts
                .Include(op => op.Product)
                .GroupBy(op => op.ProductId)
                .Select(g => new ProductPopularityViewModel
                {
                    ProductName = g.First().Product.Name,
                    TotalQuantity = g.Sum(op => op.Quantity)
                })
                .OrderByDescending(p => p.TotalQuantity)
                .ToListAsync();

            // Данные по услугам
            var servicePopularity = await _context.OrderServices
                .Include(os => os.Service)
                .GroupBy(os => os.ServiceId)
                .Select(g => new ServicePopularityViewModel
                {
                    ServiceName = g.First().Service.Name,
                    TotalUsage = g.Count()
                })
                .OrderByDescending(s => s.TotalUsage)
                .ToListAsync();

            var report = new PopularityReportViewModel
            {
                ProductPopularity = productPopularity,
                ServicePopularity = servicePopularity
            };

            return View(report);
        }
        // Отчёт о прибыли
        public async Task<IActionResult> ProfitReport()
        {
            // Прибыль по товарам
            var productProfits = await _context.OrderProducts
                .Include(op => op.Product)
                .GroupBy(op => op.ProductId)
                .Select(g => new ProfitProductViewModel
                {
                    ProductName = g.First().Product.Name,
                    TotalQuantity = g.Sum(op => op.Quantity),
                    TotalProfit = g.Sum(op => op.Quantity * op.Product.Price)
                })
                .OrderByDescending(p => p.TotalProfit)
                .ToListAsync();

            // Прибыль по услугам
            var serviceProfits = await _context.OrderServices
                .Include(os => os.Service)
                .GroupBy(os => os.ServiceId)
                .Select(g => new ProfitServiceViewModel
                {
                    ServiceName = g.First().Service.Name,
                    TotalUsage = g.Count(),
                    TotalProfit = g.Sum(os => os.Service.Price)
                })
                .OrderByDescending(s => s.TotalProfit)
                .ToListAsync();

            // Общая прибыль
            var totalProfit = productProfits.Sum(p => p.TotalProfit) + serviceProfits.Sum(s => s.TotalProfit);

            var report = new ProfitReportViewModel
            {
                ProductProfits = productProfits,
                ServiceProfits = serviceProfits,
                TotalProfit = totalProfit
            };

            return View(report);
        }
    }
}
