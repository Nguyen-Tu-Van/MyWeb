#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyWeb.Models;

namespace MyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "UserOnly")]
    public class UserController : Controller
    {
        private readonly MainDbContext _context;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public UserController(MainDbContext context, SignInManager<User> signInManager, UserManager<User> userManager)
        {
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // GET: Admin/User
        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
        }

        // GET: Admin/User/Details/5
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Admin/User/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/User/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Email,Birthday")] User user)
        {
            if (ModelState.IsValid)
            {
                var check = _context.Users.Where(u => u.Email == user.Email).FirstOrDefault();
                if (check == null)
                {
                    var userApp = new User { UserName = user.Email, Email = user.Email, Name = user.Name ,Birthday = user.Birthday};
                    var result = await _userManager.CreateAsync(userApp, Request.Form["Password"]);
                    if (result.Succeeded)
                    {
                        TempData["success"] = $"Đăng kí tài khoản thành công {user.UserName}";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData["success"] = $"Error : Tài khoản chưa được tạo";
                    }
                }
                else
                {
                    TempData["success"] = $"Error : Tài khoản đã tồn tại";
                }
            }
            return View(user);
        }

        // GET: Admin/User/Edit/5
        public async Task<IActionResult> Edit(string? id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var roles = _context.Roles.ToList();
            ViewData["roles"] = roles;
            var userRole = _context.UserRoles.ToList();
            var listRoleId = (from a in userRole
                                     where a.UserId == id
                                     select a.RoleId).ToList();
            var permissions = (from b in _context.RoleClaims
                              where listRoleId.Contains(b.RoleId)
                              group b by new { b.ClaimType,b.ClaimValue } into gr
                              select gr.Key).ToList();
            List<IdentityUserClaim<string>> permissionsPrivate = _context.UserClaims.Where(c=>c.UserId==id).ToList();
            ViewData["listRoleId"] = listRoleId;
            ViewData["permissions"] = permissions;
            ViewData["permissionsPrivate"] = permissionsPrivate;

            return View(user);
        }

        // POST: Admin/User/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Name,Email,Birthday")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var userM = await _userManager.FindByIdAsync(id);
                    if(userM == null) return NotFound("No find");
                    userM.Name = user.Name;
                    userM.Birthday = user.Birthday;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    return NotFound("Error");
                }
                TempData["success"] = "Cập nhật thành công";
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Admin/User/Delete/5
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Admin/User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _context.Users.FindAsync(id);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        public async Task<IActionResult> AddRoleUser(string id)
        {
            var user = _context.Users.Find(id);
            if(user==null) return NotFound("Không tìm thấy user");
            var listRoleId = Request.Form["roles"];

            var oldRoles = (from r in _context.UserRoles
                           where r.UserId == id
                           select r);
            _context.UserRoles.RemoveRange(oldRoles);
            _context.SaveChanges();

            var resultAdd = await _userManager.AddToRolesAsync(user, listRoleId);
            if (resultAdd.Succeeded)
            {
                TempData["success"] = "Cập nhật thành công";
            }
            else TempData["success"] = "Error : Cập nhật không thành công";

            return RedirectToAction("Edit", new {id=id});
        }    

        private bool UserExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
