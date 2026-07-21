using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YunChenShipping.Data;
using YunChenShipping.Models;

namespace YunChenShipping.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Customer
        public async Task<IActionResult> Index(string searchString, bool? isActive)
        {
            var customers = from c in _context.Customers
                           select c;

            if (!string.IsNullOrEmpty(searchString))
            {
                customers = customers.Where(c =>
                    c.Name.Contains(searchString) ||
                    (c.Phone != null && c.Phone.Contains(searchString)) ||
                    (c.TaxId != null && c.TaxId.Contains(searchString)));
            }

            if (isActive.HasValue)
            {
                customers = customers.Where(c => c.IsActive == isActive.Value);
            }

            return View(await customers.OrderByDescending(c => c.CreatedAt).ToListAsync());
        }

        // GET: Customer/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .Include(c => c.Addresses)
                .Include(c => c.Contacts)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // GET: Customer/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Customer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Name,Phone,Fax,TaxId,PaymentMethod,Remarks")] Customer customer,
            string[] addresses,
            bool[] addressIsDefault,
            string[] contactNames,
            string[] contactPhones,
            string[] contactTitles,
            bool[] contactIsPrimary)
        {
            if (ModelState.IsValid)
            {
                customer.CreatedAt = DateTime.Now;
                customer.CreatedBy = User.Identity?.Name;
                _context.Add(customer);
                await _context.SaveChangesAsync();

                // 新增多個地址
                if (addresses != null && addresses.Length > 0)
                {
                    for (int i = 0; i < addresses.Length; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(addresses[i]))
                        {
                            var customerAddress = new CustomerAddress
                            {
                                CustomerId = customer.Id,
                                Address = addresses[i],
                                IsDefault = addressIsDefault != null && i < addressIsDefault.Length ? addressIsDefault[i] : i == 0
                            };
                            _context.CustomerAddresses.Add(customerAddress);
                        }
                    }
                }

                // 新增多個聯絡人
                if (contactNames != null && contactNames.Length > 0)
                {
                    for (int i = 0; i < contactNames.Length; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(contactNames[i]))
                        {
                            var contact = new CustomerContact
                            {
                                CustomerId = customer.Id,
                                Name = contactNames[i],
                                Phone = contactPhones != null && i < contactPhones.Length ? contactPhones[i] : null,
                                Title = contactTitles != null && i < contactTitles.Length ? contactTitles[i] : null,
                                IsPrimary = contactIsPrimary != null && i < contactIsPrimary.Length ? contactIsPrimary[i] : i == 0
                            };
                            _context.CustomerContacts.Add(contact);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "客戶新增成功！";
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // GET: Customer/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .Include(c => c.Addresses)
                .Include(c => c.Contacts)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // POST: Customer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,Name,Phone,Fax,TaxId,PaymentMethod,Remarks,IsActive")] Customer customer,
            string[] addresses,
            bool[] addressIsDefault,
            string[] contactNames,
            string[] contactPhones,
            string[] contactTitles,
            bool[] contactIsPrimary)
        {
            if (id != customer.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingCustomer = await _context.Customers.FindAsync(id);
                    if (existingCustomer == null)
                    {
                        return NotFound();
                    }

                    // 更新客戶基本資料
                    existingCustomer.Name = customer.Name;
                    existingCustomer.Phone = customer.Phone;
                    existingCustomer.Fax = customer.Fax;
                    existingCustomer.TaxId = customer.TaxId;
                    existingCustomer.PaymentMethod = customer.PaymentMethod;
                    existingCustomer.Remarks = customer.Remarks;
                    existingCustomer.IsActive = customer.IsActive;

                    // 更新地址
                    var existingAddresses = await _context.CustomerAddresses
                        .Where(a => a.CustomerId == id)
                        .ToListAsync();
                    _context.CustomerAddresses.RemoveRange(existingAddresses);

                    if (addresses != null && addresses.Length > 0)
                    {
                        for (int i = 0; i < addresses.Length; i++)
                        {
                            if (!string.IsNullOrWhiteSpace(addresses[i]))
                            {
                                var customerAddress = new CustomerAddress
                                {
                                    CustomerId = id,
                                    Address = addresses[i],
                                    IsDefault = addressIsDefault != null && i < addressIsDefault.Length ? addressIsDefault[i] : i == 0
                                };
                                _context.CustomerAddresses.Add(customerAddress);
                            }
                        }
                    }

                    // 更新聯絡人
                    var existingContacts = await _context.CustomerContacts
                        .Where(c => c.CustomerId == id)
                        .ToListAsync();
                    _context.CustomerContacts.RemoveRange(existingContacts);

                    if (contactNames != null && contactNames.Length > 0)
                    {
                        for (int i = 0; i < contactNames.Length; i++)
                        {
                            if (!string.IsNullOrWhiteSpace(contactNames[i]))
                            {
                                var contact = new CustomerContact
                                {
                                    CustomerId = id,
                                    Name = contactNames[i],
                                    Phone = contactPhones != null && i < contactPhones.Length ? contactPhones[i] : null,
                                    Title = contactTitles != null && i < contactTitles.Length ? contactTitles[i] : null,
                                    IsPrimary = contactIsPrimary != null && i < contactIsPrimary.Length ? contactIsPrimary[i] : i == 0
                                };
                                _context.CustomerContacts.Add(contact);
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "客戶更新成功！";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customer.Id))
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
            return View(customer);
        }

        // POST: Customer/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                // 軟刪除 - 設為停用
                customer.IsActive = false;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "客戶已停用！";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.Id == id);
        }

        // API: 取得客戶地址
        [HttpGet]
        public async Task<IActionResult> GetAddresses(int customerId)
        {
            var addresses = await _context.CustomerAddresses
                .Where(a => a.CustomerId == customerId)
                .Select(a => new { a.Id, a.Address, a.IsDefault })
                .ToListAsync();

            return Json(addresses);
        }

        // API: 取得客戶聯絡人
        [HttpGet]
        public async Task<IActionResult> GetContacts(int customerId)
        {
            var contacts = await _context.CustomerContacts
                .Where(c => c.CustomerId == customerId)
                .Select(c => new { c.Id, c.Name, c.Phone, c.Title, c.IsPrimary })
                .ToListAsync();

            return Json(contacts);
        }
    }
}
