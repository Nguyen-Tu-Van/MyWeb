using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyWeb.Models;
using System.ComponentModel.DataAnnotations;

namespace MyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
/*    [Authorize(Policy = "RoleOnly")]*/
    public class RoleController : Controller
    {
        private MainDbContext _context;
        private RoleManager<IdentityRole> _roleManager;
        public RoleController(MainDbContext context, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _roleManager = roleManager;
        }
        // GET: RoleController

        public class InputModel
        {
            [Display(Name = "Tên vai trò")]
            [Required(ErrorMessage = "Bạn chưa nhập tên vai trò")]
            [StringLength(255, MinimumLength = 3, ErrorMessage = "Độ dài không chính xác")]
            public string Name { get; set; }
        }
        [BindProperty]
        public InputModel Input { get; set; }
        public ActionResult Index()
        {
            var roles = _roleManager.Roles.ToList();

            ViewData["roles"] = roles;

            return View();
        }

        // GET: RoleController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: RoleController/Create
        public ActionResult Create()
        {
            return View(new InputModel());
        }

        // POST: RoleController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(InputModel Input)
        {
            try
            {
                var role = new IdentityRole(Input.Name);
                var result = await _roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    TempData["message"] = $"Tạo thành công vai trò {role.Name}";
                    return RedirectToAction("Index");
                }
                else TempData["message"] = $"Error : Vai trò {role.Name} đã tồn tại";
                return RedirectToAction("Create");
            }
            catch
            {
                TempData["message"] = $"Error : Tạo vai trò thất bại";
                return View();
            }
        }

        // GET: RoleController/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role != null)
            {
                Input = new InputModel { Name = role.Name };
                List<IdentityRoleClaim<string>> permissions = _context.RoleClaims.Where(r => r.RoleId == role.Id).ToList();
                ViewData["permissions"] = permissions;
                ViewData["id"] = id;
            }
            else return NotFound("Không tìm thấy trang");
            return View(Input);
        }

        // POST: RoleController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(string id, InputModel Input)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(id);
                if(role!= null)
                {
                    role.Name = Input.Name;
                    var result = await _roleManager.UpdateAsync(role);
                    if (result.Succeeded)
                    {
                        TempData["message"] = $"Cập nhật thành công vai trò {role.Name}";
                    }
                    else TempData["message"] = $"Error : Vai trò {role.Name} đã tồn tại";
                }    
            }
            catch
            {
                TempData["message"] = $"Có lỗi xảy ra";
                return RedirectToAction("Edit");
            }
            return RedirectToAction("Edit");
        }

        // GET: RoleController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: RoleController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string id, IFormCollection collection)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(id);
                if (role == null) return NotFound("Không tìm thấy chức vụ");
                await _roleManager.DeleteAsync(role);
                TempData["message"] = $"Xóa thành công vai trò {role.Name}";

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
