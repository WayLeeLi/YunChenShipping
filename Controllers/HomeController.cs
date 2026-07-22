using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YunChenShipping.Data;
using YunChenShipping.Models;

namespace YunChenShipping.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    [Authorize]
    public async Task<IActionResult> Index()
    {
        var today = DateTime.Today;

        // 今日統計
        ViewBag.TodayCustomers = await _context.Customers
            .CountAsync(c => c.CreatedAt >= today);
        ViewBag.TodayProducts = await _context.Products
            .CountAsync(p => p.CreatedAt >= today);
        ViewBag.TodayOrders = await _context.ShippingOrders
            .CountAsync(o => o.CreatedAt >= today);

        // 最近出貨單
        ViewBag.RecentOrders = await _context.ShippingOrders
            .Include(o => o.Customer)
            .OrderByDescending(o => o.OrderDate)
            .Take(5)
            .Select(o => new
            {
                o.Id,
                o.OrderNo,
                CustomerName = o.Customer!.Name,
                o.OrderDate,
                o.Total,
                o.Status
            })
            .ToListAsync();

        // 最近新增客戶
        ViewBag.RecentCustomers = await _context.Customers
            .OrderByDescending(c => c.CreatedAt)
            .Take(5)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Phone,
                c.ContactPerson,
                c.CreatedAt
            })
            .ToListAsync();

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
