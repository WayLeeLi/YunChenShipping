using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YunChenShipping.Data;
using YunChenShipping.Models;

namespace YunChenShipping.Controllers
{
    [Authorize]
    public class TaxCategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TaxCategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TaxCategory
        public async Task<IActionResult> Index()
        {
            return View(await _context.TaxCategories.OrderBy(t => t.SortOrder).ToListAsync());
        }

        // GET: TaxCategory/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TaxCategory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,TaxRate,SortOrder")] TaxCategory taxCategory)
        {
            if (ModelState.IsValid)
            {
                taxCategory.CreatedAt = DateTime.Now;
                _context.Add(taxCategory);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "營業稅新增成功！";
                return RedirectToAction(nameof(Index));
            }
            return View(taxCategory);
        }

        // GET: TaxCategory/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var taxCategory = await _context.TaxCategories.FindAsync(id);
            if (taxCategory == null) return NotFound();

            return View(taxCategory);
        }

        // POST: TaxCategory/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,TaxRate,SortOrder")] TaxCategory taxCategory)
        {
            if (id != taxCategory.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.TaxCategories.FindAsync(id);
                    if (existing == null) return NotFound();

                    existing.Name = taxCategory.Name;
                    existing.TaxRate = taxCategory.TaxRate;
                    existing.SortOrder = taxCategory.SortOrder;

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "營業稅更新成功！";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.TaxCategories.AnyAsync(e => e.Id == id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(taxCategory);
        }

        // POST: TaxCategory/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var taxCategory = await _context.TaxCategories.FindAsync(id);
            if (taxCategory != null)
            {
                _context.TaxCategories.Remove(taxCategory);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "營業稅已刪除！";
            }
            return RedirectToAction(nameof(Index));
        }

        // API: 取得稅目列表
        [HttpGet]
        public async Task<IActionResult> GetTaxCategories()
        {
            var categories = await _context.TaxCategories
                .Select(t => new { t.Id, t.Name, t.TaxRate, t.SortOrder })
                .OrderBy(t => t.SortOrder)
                .ToListAsync();
            return Json(categories);
        }
    }
}
