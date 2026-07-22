using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using YunChenShipping.Data;
using YunChenShipping.Models;

namespace YunChenShipping.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public UserManagementController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // 所有可用的選單
        private static readonly List<MenuDefinition> AllMenus = new List<MenuDefinition>
        {
            new MenuDefinition { Key = "Home", Name = "首頁", Controller = "Home", Action = "Index", Group = "主選單" },
            new MenuDefinition { Key = "Customer", Name = "客戶管理", Controller = "Customer", Action = "Index", Group = "業務管理" },
            new MenuDefinition { Key = "Product", Name = "產品管理", Controller = "Product", Action = "Index", Group = "業務管理" },
            new MenuDefinition { Key = "ShippingOrder", Name = "出貨單管理", Controller = "ShippingOrder", Action = "Index", Group = "業務管理" },
            new MenuDefinition { Key = "SystemSetting", Name = "系統設定", Controller = "SystemSetting", Action = "Index", Group = "系統管理" },
            new MenuDefinition { Key = "UserManagement", Name = "用戶管理", Controller = "UserManagement", Action = "Index", Group = "系統管理" },
            new MenuDefinition { Key = "RoleManagement", Name = "角色管理", Controller = "UserManagement", Action = "Roles", Group = "系統管理" },
        };

        // GET: UserManagement
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    UserName = user.UserName ?? "",
                    ChineseName = user.ChineseName,
                    Roles = string.Join(", ", roles),
                    EmailConfirmed = user.EmailConfirmed,
                    LockoutEnabled = user.LockoutEnabled
                });
            }

            return View(userViewModels);
        }

        // GET: UserManagement/Create
        public IActionResult Create()
        {
            ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
            return View();
        }

        // POST: UserManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    ChineseName = model.ChineseName,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.Role))
                    {
                        await _userManager.AddToRoleAsync(user, model.Role);
                    }
                    TempData["SuccessMessage"] = "用戶建立成功！";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
            return View(model);
        }

        // GET: UserManagement/Edit/5
        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? "",
                ChineseName = user.ChineseName,
                CurrentRole = roles.FirstOrDefault() ?? ""
            };

            ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
            return View(model);
        }

        // POST: UserManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditUserViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null) return NotFound();

                user.Email = model.Email;
                user.UserName = model.Email;
                user.ChineseName = model.ChineseName;
                await _userManager.UpdateAsync(user);

                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!string.IsNullOrEmpty(model.CurrentRole))
                {
                    await _userManager.AddToRoleAsync(user, model.CurrentRole);
                }

                if (!string.IsNullOrEmpty(model.NewPassword))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
                }

                TempData["SuccessMessage"] = "用戶更新成功！";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
            return View(model);
        }

        // POST: UserManagement/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                if (user.UserName == User.Identity?.Name)
                {
                    TempData["ErrorMessage"] = "無法刪除自己的帳號！";
                    return RedirectToAction(nameof(Index));
                }
                await _userManager.DeleteAsync(user);
                TempData["SuccessMessage"] = "用戶已刪除！";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: UserManagement/Roles
        public async Task<IActionResult> Roles()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return View(roles);
        }

        // POST: UserManagement/CreateRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            if (!string.IsNullOrEmpty(roleName))
            {
                var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
                if (result.Succeeded)
                {
                    foreach (var menu in AllMenus)
                    {
                        _context.RolePermissions.Add(new RolePermission
                        {
                            RoleName = roleName,
                            MenuKey = menu.Key,
                            MenuName = menu.Name,
                            Controller = menu.Controller,
                            Action = menu.Action,
                            IsVisible = true,
                            Group = menu.Group
                        });
                    }
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "角色建立成功！";
                }
                else
                {
                    TempData["ErrorMessage"] = "角色建立失敗！";
                }
            }
            return RedirectToAction(nameof(Roles));
        }

        // POST: UserManagement/DeleteRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role != null)
            {
                if (role.Name == "Admin" || role.Name == "Sales" || role.Name == "Accounting" || role.Name == "Warehouse")
                {
                    TempData["ErrorMessage"] = "無法刪除系統預設角色！";
                    return RedirectToAction(nameof(Roles));
                }
                var permissions = await _context.RolePermissions.Where(p => p.RoleName == role.Name).ToListAsync();
                _context.RolePermissions.RemoveRange(permissions);
                await _roleManager.DeleteAsync(role);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "角色已刪除！";
            }
            return RedirectToAction(nameof(Roles));
        }

        // GET: UserManagement/EditRolePermissions/roleName
        public async Task<IActionResult> EditRolePermissions(string roleName)
        {
            if (string.IsNullOrEmpty(roleName)) return NotFound();

            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null) return NotFound();

            var permissions = await _context.RolePermissions
                .Where(p => p.RoleName == roleName)
                .ToListAsync();

            if (!permissions.Any())
            {
                foreach (var menu in AllMenus)
                {
                    permissions.Add(new RolePermission
                    {
                        RoleName = roleName,
                        MenuKey = menu.Key,
                        MenuName = menu.Name,
                        Controller = menu.Controller,
                        Action = menu.Action,
                        IsVisible = true,
                        Group = menu.Group
                    });
                }
            }

            var model = new RolePermissionViewModel
            {
                RoleName = roleName,
                Permissions = permissions.OrderBy(p => p.Group).ThenBy(p => p.MenuName).ToList()
            };

            return View(model);
        }

        // POST: UserManagement/SaveRolePermissions
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveRolePermissions(string roleName, string[] menuKeys, string[] isVisible)
        {
            if (string.IsNullOrEmpty(roleName))
            {
                TempData["ErrorMessage"] = "角色名稱不能為空！";
                return RedirectToAction(nameof(Roles));
            }

            // 刪除舊權限
            var oldPermissions = await _context.RolePermissions
                .Where(p => p.RoleName == roleName)
                .ToListAsync();
            _context.RolePermissions.RemoveRange(oldPermissions);

            // 新增新權限（menuKeys 和 isVisible 一一對應）
            if (menuKeys != null && isVisible != null)
            {
                for (int i = 0; i < menuKeys.Length; i++)
                {
                    var menu = AllMenus.FirstOrDefault(m => m.Key == menuKeys[i]);
                    if (menu != null)
                    {
                        _context.RolePermissions.Add(new RolePermission
                        {
                            RoleName = roleName,
                            MenuKey = menu.Key,
                            MenuName = menu.Name,
                            Controller = menu.Controller,
                            Action = menu.Action,
                            IsVisible = isVisible[i] == "true",
                            Group = menu.Group
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "權限儲存成功！";
            return RedirectToAction(nameof(EditRolePermissions), new { roleName });
        }
    }

    public class MenuDefinition
    {
        public string Key { get; set; } = "";
        public string Name { get; set; } = "";
        public string Controller { get; set; } = "";
        public string Action { get; set; } = "";
        public string Group { get; set; } = "";
    }

    public class UserViewModel
    {
        public string Id { get; set; } = "";
        public string Email { get; set; } = "";
        public string UserName { get; set; } = "";
        public string? ChineseName { get; set; }
        public string Roles { get; set; } = "";
        public bool EmailConfirmed { get; set; }
        public bool LockoutEnabled { get; set; }
    }

    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "帳號為必填")]
        [EmailAddress(ErrorMessage = "請輸入有效的電子郵件")]
        [Display(Name = "帳號(電子郵件)")]
        public string Email { get; set; } = "";

        [StringLength(50)]
        [Display(Name = "中文名")]
        public string? ChineseName { get; set; }

        [Required(ErrorMessage = "密碼為必填")]
        [StringLength(100, ErrorMessage = "{0} 必須至少包含 {2} 個字元。", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "密碼")]
        public string Password { get; set; } = "";

        [DataType(DataType.Password)]
        [Display(Name = "確認密碼")]
        [Compare("Password", ErrorMessage = "密碼和確認密碼不相符。")]
        public string ConfirmPassword { get; set; } = "";

        [Display(Name = "角色")]
        public string? Role { get; set; }
    }

    public class EditUserViewModel
    {
        public string Id { get; set; } = "";

        [Required(ErrorMessage = "帳號為必填")]
        [EmailAddress(ErrorMessage = "請輸入有效的電子郵件")]
        [Display(Name = "帳號(電子郵件)")]
        public string Email { get; set; } = "";

        [StringLength(50)]
        [Display(Name = "中文名")]
        public string? ChineseName { get; set; }

        [Display(Name = "角色")]
        public string CurrentRole { get; set; } = "";

        [Display(Name = "新密碼(留空則不修改)")]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }
    }

    public class RolePermissionViewModel
    {
        public string RoleName { get; set; } = "";
        public List<RolePermission> Permissions { get; set; } = new List<RolePermission>();
    }
}
