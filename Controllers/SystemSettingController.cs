using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using YunChenShipping.Data;
using YunChenShipping.Models;

namespace YunChenShipping.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SystemSettingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SystemSettingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: SystemSetting
        public async Task<IActionResult> Index()
        {
            var settings = await _context.SystemSettings.ToListAsync();

            // 若無設定則初始化預設值
            if (!settings.Any())
            {
                await InitializeDefaultSettings();
                settings = await _context.SystemSettings.ToListAsync();
            }

            var model = new SystemSettingViewModel
            {
                CompanyName = GetSetting(settings, "CompanyName", "允晨科技有限公司"),
                CompanyAddress = GetSetting(settings, "CompanyAddress", ""),
                CompanyPhone = GetSetting(settings, "CompanyPhone", ""),
                CompanyFax = GetSetting(settings, "CompanyFax", ""),
                OrderNoPrefix = GetSetting(settings, "OrderNoPrefix", "SO"),
                TaxRate = decimal.Parse(GetSetting(settings, "TaxRate", "5")),
                PrintMarginTop = int.Parse(GetSetting(settings, "PrintMarginTop", "10")),
                PrintMarginLeft = int.Parse(GetSetting(settings, "PrintMarginLeft", "10"))
            };

            return View(model);
        }

        // POST: SystemSetting/Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(SystemSettingViewModel model)
        {
            if (ModelState.IsValid)
            {
                await SaveSetting("CompanyName", model.CompanyName, "公司名稱", "Company");
                await SaveSetting("CompanyAddress", model.CompanyAddress, "公司地址", "Company");
                await SaveSetting("CompanyPhone", model.CompanyPhone, "公司電話", "Company");
                await SaveSetting("CompanyFax", model.CompanyFax, "公司傳真", "Company");
                await SaveSetting("OrderNoPrefix", model.OrderNoPrefix, "出貨單號前綴", "OrderNo");
                await SaveSetting("TaxRate", model.TaxRate.ToString(), "營業稅率(%)", "Tax");
                await SaveSetting("PrintMarginTop", model.PrintMarginTop.ToString(), "列印上邊距(mm)", "Print");
                await SaveSetting("PrintMarginLeft", model.PrintMarginLeft.ToString(), "列印左邊距(mm)", "Print");

                TempData["SuccessMessage"] = "系統設定儲存成功！";
            }

            return RedirectToAction(nameof(Index));
        }

        // API: 取得系統設定
        [HttpGet]
        public async Task<IActionResult> GetSetting(string key)
        {
            var setting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.SettingKey == key);

            if (setting == null)
            {
                return NotFound();
            }

            return Json(new { key = setting.SettingKey, value = setting.SettingValue });
        }

        // API: 取得所有設定
        [HttpGet]
        public async Task<IActionResult> GetAllSettings()
        {
            var settings = await _context.SystemSettings
                .Select(s => new { s.SettingKey, s.SettingValue })
                .ToListAsync();

            return Json(settings);
        }

        private async Task SaveSetting(string key, string value, string description, string category)
        {
            var setting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.SettingKey == key);

            if (setting == null)
            {
                setting = new SystemSetting
                {
                    SettingKey = key,
                    SettingValue = value,
                    Description = description,
                    Category = category
                };
                _context.SystemSettings.Add(setting);
            }
            else
            {
                setting.SettingValue = value;
            }

            await _context.SaveChangesAsync();
        }

        private string GetSetting(List<SystemSetting> settings, string key, string defaultValue)
        {
            var setting = settings.FirstOrDefault(s => s.SettingKey == key);
            return setting?.SettingValue ?? defaultValue;
        }

        private async Task InitializeDefaultSettings()
        {
            var defaults = new Dictionary<string, (string Value, string Desc, string Cat)>
            {
                { "CompanyName", ("允晨科技有限公司", "公司名稱", "Company") },
                { "CompanyAddress", ("", "公司地址", "Company") },
                { "CompanyPhone", ("", "公司電話", "Company") },
                { "CompanyFax", ("", "公司傳真", "Company") },
                { "OrderNoPrefix", ("SO", "出貨單號前綴", "OrderNo") },
                { "TaxRate", ("5", "營業稅率(%)", "Tax") },
                { "PrintMarginTop", ("10", "列印上邊距(mm)", "Print") },
                { "PrintMarginLeft", ("10", "列印左邊距(mm)", "Print") }
            };

            foreach (var kv in defaults)
            {
                _context.SystemSettings.Add(new SystemSetting
                {
                    SettingKey = kv.Key,
                    SettingValue = kv.Value.Value,
                    Description = kv.Value.Desc,
                    Category = kv.Value.Cat
                });
            }

            await _context.SaveChangesAsync();
        }
    }

    public class SystemSettingViewModel
    {
        [Display(Name = "公司名稱")]
        public string CompanyName { get; set; } = "";

        [Display(Name = "公司地址")]
        public string CompanyAddress { get; set; } = "";

        [Display(Name = "公司電話")]
        public string CompanyPhone { get; set; } = "";

        [Display(Name = "公司傳真")]
        public string CompanyFax { get; set; } = "";

        [Display(Name = "出貨單號前綴")]
        public string OrderNoPrefix { get; set; } = "SO";

        [Display(Name = "營業稅率(%)")]
        public decimal TaxRate { get; set; } = 5;

        [Display(Name = "列印上邊距(mm)")]
        public int PrintMarginTop { get; set; } = 10;

        [Display(Name = "列印左邊距(mm)")]
        public int PrintMarginLeft { get; set; } = 10;
    }
}
