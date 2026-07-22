using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YunChenShipping.Data;
using YunChenShipping.Models;

namespace YunChenShipping.Controllers
{
    [Authorize]
    public class ShippingOrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ShippingOrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: ShippingOrder
        public async Task<IActionResult> Index(
            string searchString,
            int? customerId,
            DateTime? startDate,
            DateTime? endDate,
            OrderStatus? status,
            int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var orders = from o in _context.ShippingOrders
                         .Include(o => o.Customer)
                        select o;

            if (!string.IsNullOrEmpty(searchString))
            {
                orders = orders.Where(o =>
                    o.OrderNo.Contains(searchString) ||
                    (o.InvoiceNo != null && o.InvoiceNo.Contains(searchString)) ||
                    (o.ReferenceNo != null && o.ReferenceNo.Contains(searchString)));
            }

            if (customerId.HasValue)
            {
                orders = orders.Where(o => o.CustomerId == customerId.Value);
            }

            if (startDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate <= endDate.Value.AddDays(1));
            }

            if (status.HasValue)
            {
                orders = orders.Where(o => o.Status == status.Value);
            }

            var sortedOrders = orders.OrderBy(o => o.SortOrder).ThenByDescending(o => o.OrderNo);
            var paginatedList = await PaginatedList<ShippingOrder>.CreateAsync(sortedOrders, pageNumber, pageSize);

            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentPage"] = pageNumber;
            ViewData["TotalPages"] = paginatedList.TotalPages;

            return View(paginatedList);
        }

        // GET: ShippingOrder/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shippingOrder = await _context.ShippingOrders
                .Include(o => o.Customer)
                .Include(o => o.Details)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (shippingOrder == null)
            {
                return NotFound();
            }

            // 獲取承辦人中文名
            if (!string.IsNullOrEmpty(shippingOrder.Handler))
            {
                var handler = await _userManager.FindByNameAsync(shippingOrder.Handler);
                ViewBag.HandlerChineseName = handler?.ChineseName ?? shippingOrder.Handler;
            }
            else
            {
                ViewBag.HandlerChineseName = "-";
            }

            return View(shippingOrder);
        }

        // GET: ShippingOrder/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Customers = await _context.Customers
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            // 生成出貨單號
            var today = DateTime.Now;
            var orderNo = $"SO-{today:yyyyMMdd}-001";

            var lastOrder = await _context.ShippingOrders
                .Where(o => o.OrderNo.StartsWith($"SO-{today:yyyyMMdd}"))
                .OrderByDescending(o => o.OrderNo)
                .FirstOrDefaultAsync();

            if (lastOrder != null)
            {
                var lastNumber = int.Parse(lastOrder.OrderNo.Split('-').Last());
                orderNo = $"SO-{today:yyyyMMdd}-{(lastNumber + 1):D3}";
            }

            var model = new ShippingOrder
            {
                OrderNo = orderNo,
                OrderDate = today,
                Status = OrderStatus.Draft,
                Handler = User.Identity?.Name
            };

            return View(model);
        }

        // POST: ShippingOrder/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("OrderNo,CustomerId,InvoiceNo,OrderDate,ReferenceNo,ProjectNo,PaymentMethod,DeliveryMethod,DeliveryAddress,Remarks,OtherExpenses,Handler,SortOrder")] ShippingOrder order,
            [FromForm] string[]? ProductIds,
            [FromForm] string[]? ProductNames,
            [FromForm] string[]? PartNos,
            [FromForm] string[]? Quantities,
            [FromForm] string[]? Units,
            [FromForm] string[]? UnitPrices,
            [FromForm] int TaxCategoryId,
            [FromForm] decimal TaxRate)
        {
            // 手動解析陣列
            var productIdList = FormArrayParser.ParseIntList(ProductIds);
            var quantityList = FormArrayParser.ParseIntList(Quantities);
            var unitPriceList = FormArrayParser.ParseDecimalList(UnitPrices);
            var productNameList = FormArrayParser.ParseStringList(ProductNames);
            var partNoList = FormArrayParser.ParseStringList(PartNos);
            var unitList = FormArrayParser.ParseStringList(Units);

            if (ModelState.IsValid)
            {
                order.CreatedAt = DateTime.Now;
                order.CreatedBy = User.Identity?.Name;
                order.Status = OrderStatus.Draft;
                order.TaxCategoryId = TaxCategoryId;
                order.TaxRate = TaxRate;

                // 計算金額
                decimal subTotal = 0;

                if (productIdList.Count > 0 && quantityList.Count > 0 && unitPriceList.Count > 0)
                {
                    for (int i = 0; i < productIdList.Count; i++)
                    {
                        if (i < quantityList.Count && i < unitPriceList.Count && quantityList[i] > 0)
                        {
                            var lineTotal = quantityList[i] * unitPriceList[i];
                            subTotal += lineTotal;
                        }
                    }
                }

                // 根據稅目計算稅金
                decimal taxAmount = 0;
                if (TaxCategoryId > 0 && TaxRate > 0)
                {
                    taxAmount = Math.Round(subTotal * TaxRate / 100, 0);
                }

                order.SubTotal = subTotal;
                order.TaxAmount = taxAmount;
                order.Total = subTotal + order.TaxAmount + order.OtherExpenses;

                _context.ShippingOrders.Add(order);
                await _context.SaveChangesAsync();

                // 新增明細
                if (productIdList.Count > 0 && quantityList.Count > 0 && unitPriceList.Count > 0)
                {
                    int lineNo = 1;
                    for (int i = 0; i < productIdList.Count; i++)
                    {
                        if (i < quantityList.Count && i < unitPriceList.Count && quantityList[i] > 0)
                        {
                            var detail = new ShippingOrderDetail
                            {
                                ShippingOrderId = order.Id,
                                LineNo = lineNo++,
                                ProductId = productIdList[i],
                                ProductName = i < productNameList.Count ? productNameList[i] : null,
                                PartNo = i < partNoList.Count ? partNoList[i] : null,
                                Quantity = quantityList[i],
                                Unit = i < unitList.Count ? unitList[i] : null,
                                UnitPrice = unitPriceList[i]
                            };
                            _context.ShippingOrderDetails.Add(detail);
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "出貨單建立成功！";
                return RedirectToAction(nameof(Details), new { id = order.Id });
            }

            ViewBag.Customers = await _context.Customers
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(order);
        }

        // GET: ShippingOrder/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shippingOrder = await _context.ShippingOrders
                .Include(o => o.Details)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (shippingOrder == null)
            {
                return NotFound();
            }

            if (shippingOrder.Status != OrderStatus.Draft)
            {
                TempData["ErrorMessage"] = "非草稿狀態無法編輯！";
                return RedirectToAction(nameof(Details), new { id });
            }

            ViewBag.Customers = await _context.Customers
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(shippingOrder);
        }

        // POST: ShippingOrder/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,OrderNo,CustomerId,InvoiceNo,OrderDate,ReferenceNo,ProjectNo,PaymentMethod,DeliveryMethod,DeliveryAddress,Remarks,OtherExpenses,Handler,SortOrder")] ShippingOrder order,
            [FromForm] string[]? ProductIds,
            [FromForm] string[]? ProductNames,
            [FromForm] string[]? PartNos,
            [FromForm] string[]? Quantities,
            [FromForm] string[]? Units,
            [FromForm] string[]? UnitPrices,
            [FromForm] int TaxCategoryId,
            [FromForm] decimal TaxRate)
        {
            // 手動解析陣列
            var productIdList = FormArrayParser.ParseIntList(ProductIds);
            var quantityList = FormArrayParser.ParseIntList(Quantities);
            var unitPriceList = FormArrayParser.ParseDecimalList(UnitPrices);
            var productNameList = FormArrayParser.ParseStringList(ProductNames);
            var partNoList = FormArrayParser.ParseStringList(PartNos);
            var unitList = FormArrayParser.ParseStringList(Units);

            if (id != order.Id)
            {
                return NotFound();
            }

            var existingOrder = await _context.ShippingOrders.FindAsync(id);
            if (existingOrder == null)
            {
                return NotFound();
            }

            if (existingOrder.Status != OrderStatus.Draft)
            {
                TempData["ErrorMessage"] = "非草稿狀態無法編輯！";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    existingOrder.CustomerId = order.CustomerId;
                    existingOrder.InvoiceNo = order.InvoiceNo;
                    existingOrder.OrderDate = order.OrderDate;
                    existingOrder.ReferenceNo = order.ReferenceNo;
                    existingOrder.ProjectNo = order.ProjectNo;
                    existingOrder.PaymentMethod = order.PaymentMethod;
                    existingOrder.DeliveryMethod = order.DeliveryMethod;
                    existingOrder.DeliveryAddress = order.DeliveryAddress;
                    existingOrder.Remarks = order.Remarks;
                    existingOrder.OtherExpenses = order.OtherExpenses;
                    existingOrder.Handler = order.Handler;
                    existingOrder.TaxCategoryId = TaxCategoryId;
                    existingOrder.TaxRate = TaxRate;
                    existingOrder.SortOrder = order.SortOrder;

                    // 計算金額
                    decimal subTotal = 0;

                    if (productIdList.Count > 0 && quantityList.Count > 0 && unitPriceList.Count > 0)
                    {
                        for (int i = 0; i < productIdList.Count; i++)
                        {
                            if (i < quantityList.Count && i < unitPriceList.Count && quantityList[i] > 0)
                            {
                                var lineTotal = quantityList[i] * unitPriceList[i];
                                subTotal += lineTotal;
                            }
                        }
                    }

                    // 根據稅目計算稅金
                    decimal taxAmount = 0;
                    if (TaxCategoryId > 0 && TaxRate > 0)
                    {
                        taxAmount = Math.Round(subTotal * TaxRate / 100, 0);
                    }

                    existingOrder.SubTotal = subTotal;
                    existingOrder.TaxAmount = taxAmount;
                    existingOrder.Total = subTotal + existingOrder.TaxAmount + existingOrder.OtherExpenses;

                    // 更新明細
                    var existingDetails = await _context.ShippingOrderDetails
                        .Where(d => d.ShippingOrderId == id)
                        .ToListAsync();
                    _context.ShippingOrderDetails.RemoveRange(existingDetails);

                    if (productIdList.Count > 0 && quantityList.Count > 0 && unitPriceList.Count > 0)
                    {
                        int lineNo = 1;
                        for (int i = 0; i < productIdList.Count; i++)
                        {
                            if (i < quantityList.Count && i < unitPriceList.Count && quantityList[i] > 0)
                            {
                                var detail = new ShippingOrderDetail
                                {
                                    ShippingOrderId = order.Id,
                                    LineNo = lineNo++,
                                    ProductId = productIdList[i],
                                    ProductName = i < productNameList.Count ? productNameList[i] : null,
                                    PartNo = i < partNoList.Count ? partNoList[i] : null,
                                    Quantity = quantityList[i],
                                    Unit = i < unitList.Count ? unitList[i] : null,
                                    UnitPrice = unitPriceList[i]
                                };
                                _context.ShippingOrderDetails.Add(detail);
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "出貨單更新成功！";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShippingOrderExists(order.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), new { id = order.Id });
            }

            ViewBag.Customers = await _context.Customers
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(order);
        }

        // POST: ShippingOrder/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
        {
            var order = await _context.ShippingOrders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            order.Status = status;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "狀態更新成功！";

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: ShippingOrder/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, string approveType)
        {
            var order = await _context.ShippingOrders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            var currentUser = User.Identity?.Name;
            var isAdmin = User.IsInRole("Admin");
            var isAccounting = User.IsInRole("Accounting");

            // 獲取用戶中文名
            var user = await _userManager.FindByNameAsync(currentUser ?? "");
            var chineseName = user?.ChineseName ?? currentUser;

            // 權限檢查
            switch (approveType)
            {
                case "manager":
                    if (!isAdmin)
                    {
                        TempData["ErrorMessage"] = "僅管理員可進行主管簽核！";
                        return RedirectToAction(nameof(Details), new { id });
                    }
                    if (order.ManagerApproved)
                    {
                        TempData["ErrorMessage"] = "主管已簽核過！";
                        return RedirectToAction(nameof(Details), new { id });
                    }
                    break;
                case "accounting":
                    if (!isAdmin && !isAccounting)
                    {
                        TempData["ErrorMessage"] = "僅管理員或會計可進行會計簽核！";
                        return RedirectToAction(nameof(Details), new { id });
                    }
                    if (!order.ManagerApproved)
                    {
                        TempData["ErrorMessage"] = "需先完成主管簽核！";
                        return RedirectToAction(nameof(Details), new { id });
                    }
                    if (order.AccountingApproved)
                    {
                        TempData["ErrorMessage"] = "會計已簽核過！";
                        return RedirectToAction(nameof(Details), new { id });
                    }
                    break;
                case "handler":
                    if (!isAdmin && currentUser != order.Handler)
                    {
                        TempData["ErrorMessage"] = "僅承辦人或管理員可進行承辦簽核！";
                        return RedirectToAction(nameof(Details), new { id });
                    }
                    if (!order.ManagerApproved || !order.AccountingApproved)
                    {
                        TempData["ErrorMessage"] = "需先完成主管及會計簽核！";
                        return RedirectToAction(nameof(Details), new { id });
                    }
                    if (order.HandlerApproved)
                    {
                        TempData["ErrorMessage"] = "承辦已簽核過！";
                        return RedirectToAction(nameof(Details), new { id });
                    }
                    break;
            }

            switch (approveType)
            {
                case "manager":
                    order.ManagerApproved = true;
                    order.ManagerName = chineseName;
                    order.ManagerApprovedAt = DateTime.Now;
                    break;
                case "accounting":
                    order.AccountingApproved = true;
                    order.AccountingName = chineseName;
                    order.AccountingApprovedAt = DateTime.Now;
                    break;
                case "handler":
                    order.HandlerApproved = true;
                    order.HandlerApprovedAt = DateTime.Now;
                    break;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "簽核成功！";

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: ShippingOrder/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var order = await _context.ShippingOrders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            order.Status = OrderStatus.Cancelled;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "出貨單已作廢！";

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: ShippingOrder/DeletePermanent/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePermanent(int id)
        {
            var order = await _context.ShippingOrders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            // 永久刪除關聯數據
            var details = await _context.ShippingOrderDetails.Where(d => d.ShippingOrderId == id).ToListAsync();
            _context.ShippingOrderDetails.RemoveRange(details);

            _context.ShippingOrders.Remove(order);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "出貨單已永久刪除！";

            return RedirectToAction(nameof(Index));
        }

        private bool ShippingOrderExists(int id)
        {
            return _context.ShippingOrders.Any(e => e.Id == id);
        }

        // API: 取得客戶資料
        [HttpGet]
        public async Task<IActionResult> GetCustomer(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.Addresses)
                .Include(c => c.Contacts)
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Phone,
                    c.PaymentMethod,
                    ContactPerson = c.Contacts.FirstOrDefault(x => x.IsPrimary) != null
                        ? c.Contacts.FirstOrDefault(x => x.IsPrimary).Name
                        : c.ContactPerson,
                    Address = c.Addresses.FirstOrDefault(a => a.IsDefault) != null
                        ? c.Addresses.FirstOrDefault(a => a.IsDefault).Address
                        : c.Addresses.FirstOrDefault().Address
                })
                .FirstOrDefaultAsync();

            if (customer == null)
            {
                return NotFound();
            }

            return Json(customer);
        }

        // API: 計算金額
        [HttpPost]
        public async Task<IActionResult> CalculateAmount(
            List<int> ProductIds,
            List<int> Quantities,
            List<decimal> UnitPrices,
            decimal otherExpenses)
        {
            decimal subTotal = 0;
            decimal taxAmount = 0;

            if (ProductIds != null && Quantities != null && UnitPrices != null)
            {
                for (int i = 0; i < ProductIds.Count; i++)
                {
                    if (Quantities[i] > 0)
                    {
                        var lineTotal = Quantities[i] * UnitPrices[i];
                        subTotal += lineTotal;

                        var product = await _context.Products.FindAsync(ProductIds[i]);
                        if (product != null && product.TaxType == TaxType.Taxable)
                        {
                            taxAmount += lineTotal * 0.05m;
                        }
                    }
                }
            }

            taxAmount = Math.Round(taxAmount, 0);
            var total = subTotal + taxAmount + otherExpenses;

            return Json(new
            {
                subTotal,
                taxAmount,
                total
            });
        }

        // GET: ShippingOrder/Copy/5
        public async Task<IActionResult> Copy(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sourceOrder = await _context.ShippingOrders
                .Include(o => o.Customer)
                .Include(o => o.Details)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (sourceOrder == null)
            {
                return NotFound();
            }

            // 生成新出貨單號
            var today = DateTime.Now;
            var orderNo = $"SO-{today:yyyyMMdd}-001";
            var lastOrder = await _context.ShippingOrders
                .Where(o => o.OrderNo.StartsWith($"SO-{today:yyyyMMdd}"))
                .OrderByDescending(o => o.OrderNo)
                .FirstOrDefaultAsync();
            if (lastOrder != null)
            {
                var lastNumber = int.Parse(lastOrder.OrderNo.Split('-').Last());
                orderNo = $"SO-{today:yyyyMMdd}-{(lastNumber + 1):D3}";
            }

            // 建立新出貨單
            var newOrder = new ShippingOrder
            {
                OrderNo = orderNo,
                CustomerId = sourceOrder.CustomerId,
                OrderDate = today,
                ReferenceNo = sourceOrder.ReferenceNo,
                ProjectNo = sourceOrder.ProjectNo,
                PaymentMethod = sourceOrder.PaymentMethod,
                DeliveryMethod = sourceOrder.DeliveryMethod,
                DeliveryAddress = sourceOrder.DeliveryAddress,
                Remarks = sourceOrder.Remarks,
                Handler = User.Identity?.Name,
                Status = OrderStatus.Draft
            };

            _context.ShippingOrders.Add(newOrder);
            await _context.SaveChangesAsync();

            // 複製明細
            foreach (var detail in sourceOrder.Details)
            {
                var newDetail = new ShippingOrderDetail
                {
                    ShippingOrderId = newOrder.Id,
                    LineNo = detail.LineNo,
                    ProductId = detail.ProductId,
                    ProductName = detail.ProductName,
                    PartNo = detail.PartNo,
                    Quantity = detail.Quantity,
                    Unit = detail.Unit,
                    UnitPrice = detail.UnitPrice
                };
                _context.ShippingOrderDetails.Add(newDetail);
            }
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "已複製出貨單！";
            return RedirectToAction(nameof(Edit), new { id = newOrder.Id });
        }

        // GET: ShippingOrder/Print/5
        public async Task<IActionResult> Print(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shippingOrder = await _context.ShippingOrders
                .Include(o => o.Customer)
                .Include(o => o.Details)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (shippingOrder == null)
            {
                return NotFound();
            }

            // 讀取系統設定
            var settings = await _context.SystemSettings.ToListAsync();
            ViewBag.CompanyName = settings.FirstOrDefault(s => s.SettingKey == "CompanyName")?.SettingValue ?? "允晨科技有限公司";
            ViewBag.CompanyAddress = settings.FirstOrDefault(s => s.SettingKey == "CompanyAddress")?.SettingValue ?? "";
            ViewBag.CompanyPhone = settings.FirstOrDefault(s => s.SettingKey == "CompanyPhone")?.SettingValue ?? "";
            ViewBag.CompanyFax = settings.FirstOrDefault(s => s.SettingKey == "CompanyFax")?.SettingValue ?? "";

            // 讀取稅目名稱
            if (shippingOrder.TaxCategoryId > 0)
            {
                var taxCategory = await _context.TaxCategories.FindAsync(shippingOrder.TaxCategoryId);
                ViewBag.TaxCategoryName = taxCategory?.Name ?? "";
            }
            else
            {
                ViewBag.TaxCategoryName = "";
            }

            return View(shippingOrder);
        }
    }

    // 擴展方法：解析表單陣列
    public static class FormArrayParser
    {
        public static List<int> ParseIntList(string[]? values)
        {
            var result = new List<int>();
            if (values == null) return result;

            foreach (var v in values)
            {
                if (int.TryParse(v, out int parsed))
                    result.Add(parsed);
            }
            return result;
        }

        public static List<decimal> ParseDecimalList(string[]? values)
        {
            var result = new List<decimal>();
            if (values == null) return result;

            foreach (var v in values)
            {
                if (decimal.TryParse(v, out decimal parsed))
                    result.Add(parsed);
            }
            return result;
        }

        public static List<string> ParseStringList(string[]? values)
        {
            var result = new List<string>();
            if (values == null) return result;

            foreach (var v in values)
            {
                result.Add(v ?? "");
            }
            return result;
        }
    }
}
