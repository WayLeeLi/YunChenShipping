using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YunChenShipping.Data;
using YunChenShipping.Models;

namespace YunChenShipping.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UnitSettingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UnitSettingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: UnitSetting
        public async Task<IActionResult> Index()
        {
            var units = await _context.SystemSettings
                .Where(s => s.Category == "Unit")
                .Select(s => s.SettingValue)
                .ToListAsync();

            if (!units.Any())
            {
                units = new List<string> { "個", "箱", "PCS", "KG", "組", "套", "台", "件" };
            }

            return View(units);
        }

        // POST: UnitSetting/Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(string[] units)
        {
            // 刪除舊的單位設定
            var oldUnits = await _context.SystemSettings
                .Where(s => s.Category == "Unit")
                .ToListAsync();
            _context.SystemSettings.RemoveRange(oldUnits);

            // 新增新的單位設定
            if (units != null)
            {
                foreach (var unit in units.Where(u => !string.IsNullOrWhiteSpace(u)))
                {
                    _context.SystemSettings.Add(new SystemSetting
                    {
                        SettingKey = $"Unit_{unit.Trim()}",
                        SettingValue = unit.Trim(),
                        Description = $"單位：{unit.Trim()}",
                        Category = "Unit"
                    });
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "單位設定儲存成功！";
            return RedirectToAction(nameof(Index));
        }

        // API: 取得單位列表
        [HttpGet]
        public async Task<IActionResult> GetUnits()
        {
            var units = await _context.SystemSettings
                .Where(s => s.Category == "Unit")
                .Select(s => s.SettingValue)
                .ToListAsync();

            if (!units.Any())
            {
                units = new List<string> { "個", "箱", "PCS", "KG", "組", "套", "台", "件" };
            }

            return Json(units);
        }
    }
}
