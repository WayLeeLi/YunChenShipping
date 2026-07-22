using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YunChenShipping.Data;
using YunChenShipping.Models;

namespace YunChenShipping.Controllers
{
    [Authorize]
    public class DeliveryMethodController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DeliveryMethodController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DeliveryMethod
        public async Task<IActionResult> Index()
        {
            return View(await _context.DeliveryMethodSettings.OrderBy(d => d.SortOrder).ToListAsync());
        }

        // GET: DeliveryMethod/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: DeliveryMethod/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,SortOrder")] DeliveryMethodSetting model)
        {
            if (ModelState.IsValid)
            {
                model.CreatedAt = DateTime.Now;
                _context.Add(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "運送方式新增成功！";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: DeliveryMethod/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var model = await _context.DeliveryMethodSettings.FindAsync(id);
            if (model == null) return NotFound();

            return View(model);
        }

        // POST: DeliveryMethod/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,SortOrder")] DeliveryMethodSetting model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.DeliveryMethodSettings.FindAsync(id);
                    if (existing == null) return NotFound();

                    existing.Name = model.Name;
                    existing.SortOrder = model.SortOrder;

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "運送方式更新成功！";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.DeliveryMethodSettings.AnyAsync(e => e.Id == id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // POST: DeliveryMethod/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var model = await _context.DeliveryMethodSettings.FindAsync(id);
            if (model != null)
            {
                _context.DeliveryMethodSettings.Remove(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "運送方式已刪除！";
            }
            return RedirectToAction(nameof(Index));
        }

        // API: 取得運送方式列表
        [HttpGet]
        public async Task<IActionResult> GetDeliveryMethods()
        {
            var methods = await _context.DeliveryMethodSettings
                .OrderBy(d => d.SortOrder)
                .Select(d => new { d.Id, d.Name })
                .ToListAsync();
            return Json(methods);
        }
    }
}
