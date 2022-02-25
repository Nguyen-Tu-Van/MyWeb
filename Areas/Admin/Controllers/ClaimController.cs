using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyWeb.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace MyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize()]
    /*    [Authorize(Policy = "ClaimOnly")]*/
    public class ClaimController : Controller
    {
        private MainDbContext _context;
        private RoleManager<IdentityRole> _roleManager;
        private UserManager<User> _userManger;
        public ClaimController(MainDbContext context, RoleManager<IdentityRole> roleManager, UserManager<User> userManger)
        {
            _context = context;
            _roleManager = roleManager;
            _userManger = userManger;
        }
        public class InputModel
        {
            [Display(Name = "Tên claim")]
            [Required(ErrorMessage = "Bạn chưa nhập tên vai trò")]
            [StringLength(255, MinimumLength = 3, ErrorMessage = "Độ dài không chính xác")]
            public string ClaimType { get; set; }

            [Display(Name = "Giá trị claim")]
            [Required(ErrorMessage = "Bạn chưa nhập tên vai trò")]
            [StringLength(255, MinimumLength = 3, ErrorMessage = "Độ dài không chính xác")]
            public string ClaimValue { get; set; }
        }
        // GET: ClaimController/Create
        public ActionResult Create(string id)
        {
            var role = _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound("Không tìm thấy role");
            ViewData["role"] = role.Result;
            return View();
        }

        // GET: ClaimController/CreatePrivate
        public ActionResult CreatePrivate(string id)
        {
            var user = _context.Users.Find(id);
            if (user == null) return NotFound("Không tìm thấy user");
            ViewData["user"] = user;
            return View();
        }

        // POST: ClaimController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(string id,InputModel Input)
        {
            try
            {
                var claimCheck = _context.RoleClaims.Where(c => c.ClaimType == Input.ClaimType && c.ClaimValue == Input.ClaimValue && c.RoleId == id).FirstOrDefault();
                if (claimCheck != null)
                {
                    TempData["message"] = $"Error : Quyền hạn {Input.ClaimType}-{Input.ClaimValue} đã tồn tại v1";
                    return RedirectToAction(nameof(Edit), "Role", new { id = id });
                }
                if (ModelState.IsValid)
                {
                    var role = await _roleManager.FindByIdAsync(id);
                    var claim = new Claim(Input.ClaimType, Input.ClaimValue);
                    var result = await _roleManager.AddClaimAsync(role, claim);
                    if (result.Succeeded)
                    {
                        TempData["message"] = $"Tạo thành công quyền hạn {Input.ClaimType}-{Input.ClaimValue}";
                    }
                    else TempData["message"] = $"Error : Quyền hạn {Input.ClaimType}-{Input.ClaimValue} đã tồn tại v2";
                }    
                return RedirectToAction(nameof(Edit), "Role", new {id=id});
            }
            catch
            {
                return View();
            }
        }

        // POST: ClaimController/CreatePrivate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreatePrivate(string id, InputModel Input)
        {
            try
            {
                var claimCheck = _context.UserClaims.Where(c => c.ClaimType == Input.ClaimType && c.ClaimValue == Input.ClaimValue && c.UserId == id).FirstOrDefault();
                if (claimCheck != null)
                {
                    TempData["message"] = $"Error : Quyền hạn {Input.ClaimType}-{Input.ClaimValue} đã tồn tại v1";
                    return RedirectToAction(nameof(Edit), "User", new { id = id });
                }
                if (ModelState.IsValid)
                {
                    var user = await _userManger.FindByIdAsync(id);
                    var claim = new Claim(Input.ClaimType, Input.ClaimValue);
                    var result = await _userManger.AddClaimAsync(user, claim);
                    if (result.Succeeded)
                    {
                        TempData["message"] = $"Tạo thành công quyền hạn {Input.ClaimType}-{Input.ClaimValue}";
                    }
                    else TempData["message"] = $"Error : Quyền hạn {Input.ClaimType}-{Input.ClaimValue} đã tồn tại v2";
                }
                return RedirectToAction(nameof(Edit), "User", new { id = id });
            }
            catch
            {
                return View();
            }
        }

        // GET: ClaimController/Edit/5
        public ActionResult Edit(int id)
        {
            var claim = _context.RoleClaims.Where(p => p.Id == id).FirstOrDefault();
            if (claim == null) return NotFound("Không tìm thấy trang");
            var role = _roleManager.FindByIdAsync(claim.RoleId);
            var Input = new InputModel { ClaimType = claim.ClaimType, ClaimValue = claim.ClaimValue };
            ViewData["role"] = role.Result;
            return View(Input);
        }

        // GET: ClaimController/Edit/5
        public ActionResult EditPrivate(int id)
        {
            var claim = _context.UserClaims.Where(p => p.Id == id).FirstOrDefault();
            if (claim == null) return NotFound("Không tìm thấy trang");
            var user = _userManger.FindByIdAsync(claim.UserId);
            var Input = new InputModel { ClaimType = claim.ClaimType, ClaimValue = claim.ClaimValue };
            ViewData["user"] = user.Result;
            return View(Input);
        }

        // POST: ClaimController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, InputModel Input)
        {
            try
            {
                var claim = _context.RoleClaims.Find(id);
                if (claim == null) return NotFound("Không tìm thấy trang");
                TempData["message"] = $"Cập nhật thành công claim {claim.ClaimType}-{claim.ClaimValue} thành {Input.ClaimType}-{Input.ClaimValue}";
                claim.ClaimType = Input.ClaimType;
                claim.ClaimValue = Input.ClaimValue;
                _context.SaveChanges();

                return RedirectToAction("Edit", "Role", new { id = claim.RoleId });
            }
            catch
            {
                TempData["message"] = $"Error : Có lỗi xảy ra";
                return RedirectToAction("Index","Role");
            }
        }

        // POST: ClaimController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPrivate(int id, InputModel Input)
        {
            try
            {
                var claim = _context.UserClaims.Find(id);
                if (claim == null) return NotFound("Không tìm thấy trang");
                TempData["message"] = $"Cập nhật thành công claim {claim.ClaimType}-{claim.ClaimValue} thành {Input.ClaimType}-{Input.ClaimValue}";
                claim.ClaimType = Input.ClaimType;
                claim.ClaimValue = Input.ClaimValue;
                _context.SaveChanges();

                return RedirectToAction("Edit", "User", new { id = claim.UserId });
            }
            catch
            {
                TempData["message"] = $"Error : Có lỗi xảy ra";
                return RedirectToAction("Index", "Role");
            }
        }

        // POST: ClaimController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                var claim = _context.RoleClaims.Find(id);
                if (claim == null) return NotFound("Không tìm thấy trang này");
                var role = _context.Roles.Find(claim.RoleId);
                _context.RoleClaims.Remove(claim);
                _context.SaveChanges();
                TempData["message"] = $"Xóa thành công quyền {claim.ClaimType}-{claim.ClaimValue}";

                return RedirectToAction("Edit", "Role", new {id=role.Id});
            }
            catch
            {
                TempData["message"] = $"Error : Có lỗi xảy ra";
                return RedirectToAction("Index"); ;
            }
        }

        // POST: ClaimController/DeletePrivate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePrivate(int id, IFormCollection collection)
        {
            try
            {
                var claim = _context.UserClaims.Find(id);
                if (claim == null) return NotFound("Không tìm thấy trang này");
                var user = _context.Users.Find(claim.UserId);
                _context.UserClaims.Remove(claim);
                _context.SaveChanges();
                TempData["message"] = $"Xóa thành công quyền {claim.ClaimType}-{claim.ClaimValue}";

                return RedirectToAction("Edit", "User", new { id = user.Id });
            }
            catch
            {
                TempData["message"] = $"Error : Có lỗi xảy ra";
                return RedirectToAction("Index"); ;
            }
        }
    }
}
