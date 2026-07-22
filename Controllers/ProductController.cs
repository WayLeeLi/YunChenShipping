using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YunChenShipping.Data;
using YunChenShipping.Models;

namespace YunChenShipping.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Product
        public async Task<IActionResult> Index(string searchString, TaxType? taxType, bool? isActive, int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var products = from p in _context.Products
                          select p;

            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p =>
                    p.Name.Contains(searchString) ||
                    p.PartNo.Contains(searchString));
            }

            if (taxType.HasValue)
            {
                products = products.Where(p => p.TaxType == taxType.Value);
            }

            if (isActive.HasValue)
            {
                products = products.Where(p => p.IsActive == isActive.Value);
            }

            var sortedProducts = products.OrderByDescending(p => p.CreatedAt);
            var paginatedList = await PaginatedList<Product>.CreateAsync(sortedProducts, pageNumber, pageSize);

            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentPage"] = pageNumber;
            ViewData["TotalPages"] = paginatedList.TotalPages;

            return View(paginatedList);
        }

        // GET: Product/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.PriceHistory.OrderByDescending(h => h.ChangedAt))
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Product/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,PartNo,Unit,StandardPrice,TaxType,Remarks")] Product product)
        {
            if (ModelState.IsValid)
            {
                product.CreatedAt = DateTime.Now;
                product.CreatedBy = User.Identity?.Name;
                _context.Add(product);
                await _context.SaveChangesAsync();

                // 記錄初始價格
                var priceHistory = new ProductPriceHistory
                {
                    ProductId = product.Id,
                    Price = product.StandardPrice,
                    ChangedAt = DateTime.Now,
                    ChangedBy = User.Identity?.Name
                };
                _context.ProductPriceHistories.Add(priceHistory);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "產品新增成功！";
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Product/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,PartNo,Unit,StandardPrice,TaxType,Remarks,IsActive")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingProduct = await _context.Products.FindAsync(id);
                    if (existingProduct == null)
                    {
                        return NotFound();
                    }

                    // 如果價格改變，記錄歷史
                    if (existingProduct.StandardPrice != product.StandardPrice)
                    {
                        var priceHistory = new ProductPriceHistory
                        {
                            ProductId = product.Id,
                            Price = product.StandardPrice,
                            ChangedAt = DateTime.Now,
                            ChangedBy = User.Identity?.Name
                        };
                        _context.ProductPriceHistories.Add(priceHistory);
                    }

                    existingProduct.Name = product.Name;
                    existingProduct.PartNo = product.PartNo;
                    existingProduct.Unit = product.Unit;
                    existingProduct.StandardPrice = product.StandardPrice;
                    existingProduct.TaxType = product.TaxType;
                    existingProduct.Remarks = product.Remarks;
                    existingProduct.IsActive = product.IsActive;

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "產品更新成功！";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // POST: Product/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                // 軟刪除 - 設為停用
                product.IsActive = false;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "產品已停用！";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Product/DeletePermanent/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePermanent(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                // 檢查是否有關聯的出貨單明細
                var hasOrders = await _context.ShippingOrderDetails.AnyAsync(d => d.ProductId == id);
                if (hasOrders)
                {
                    TempData["ErrorMessage"] = "此產品已被出貨單使用，無法刪除！";
                    return RedirectToAction(nameof(Index));
                }

                // 永久刪除關聯數據
                var priceHistory = await _context.ProductPriceHistories.Where(h => h.ProductId == id).ToListAsync();
                _context.ProductPriceHistories.RemoveRange(priceHistory);

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "產品已永久刪除！";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

        // API: 取得產品資訊
        [HttpGet]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Products
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.PartNo,
                    p.Unit,
                    p.StandardPrice,
                    p.TaxType
                })
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound();
            }

            return Json(product);
        }

        // API: 搜尋產品
        [HttpGet]
        public async Task<IActionResult> Search(string term)
        {
            var query = _context.Products.Where(p => p.IsActive);

            if (!string.IsNullOrEmpty(term))
            {
                query = query.Where(p => p.PartNo.Contains(term) || p.Name.Contains(term));
            }

            var products = await query
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.PartNo,
                    p.Unit,
                    p.StandardPrice,
                    p.TaxType
                })
                .OrderBy(p => p.PartNo)
                .Take(50)
                .ToListAsync();

            return Json(products);
        }

        // API: 取得產品價格歷史
        [HttpGet]
        public async Task<IActionResult> GetPriceHistory(int productId)
        {
            var history = await _context.ProductPriceHistories
                .Where(h => h.ProductId == productId)
                .OrderByDescending(h => h.ChangedAt)
                .Select(h => new
                {
                    h.Price,
                    h.ChangedAt,
                    h.ChangedBy
                })
                .ToListAsync();

            return Json(history);
        }
    }
}
