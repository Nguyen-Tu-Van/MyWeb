#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyWeb.Models;
using Slugify;

namespace MyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "ProductOnly")]
    public class CategoryController : Controller
    {
        private readonly MainDbContext _context;

        public CategoryController(MainDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Category
        public async Task<IActionResult> Index()
        {
            var mainDbContext = _context.Categories.Include(c => c.Parent).Include(c => c.ChildCategories).ToList();
            var kq = mainDbContext.Where(c => c.ParentId == null);
            return View(kq);
        }

        // GET: Admin/Category/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.Parent)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Admin/Category/Create
        public IActionResult Create()
        {
            //var categories = _context.Categories.ToList();
            //categories.Insert(0, new Category { Id = -1, Name = "Không có danh mục cha" });
            //ViewData["ParentId"] = new SelectList(categories, "Id", "Name");
            var categories = _context.Categories.Include(c => c.Parent).Include(c => c.ChildCategories).ToList();
            ViewData["list"] = categories.Where(c => c.ParentId == null);
            return View();
        }

        // POST: Admin/Category/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,ParentId")] Category category)
        {
            if (ModelState.IsValid)
            {
                if(category.ParentId==-1) category.ParentId = null;
                category.Slug = createSLug(category.Name);
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                errors.ToList().ForEach(x => Console.WriteLine(x.ErrorMessage));
            }
            var categories = _context.Categories.Include(c => c.Parent).Include(c => c.ChildCategories).ToList();
            ViewData["list"] = categories.Where(c => c.ParentId == null);
            return View(category);
        }

        // GET: Admin/Category/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            /*var categories = _context.Categories.Where(c => c.Id != id).ToList();
            categories.Insert(0, new Category { Id=-1,Name="Không có danh mục cha"});
            ViewData["ParentId"] = new SelectList(categories, "Id", "Name", category.Id);*/
            var categories = _context.Categories.Include(c => c.Parent).Include(c => c.ChildCategories).ToList();
            ViewData["list"] = categories.Where(c => c.ParentId == null);

            return View(category);
        }

        // POST: Admin/Category/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Slug,Description,ParentId")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (category.ParentId == -1) category.ParentId = null;
                    category.Slug = createSLug(category.Name);
                    _context.Add(category);
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
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
            var categories = _context.Categories.Include(c => c.Parent).Include(c => c.ChildCategories).ToList();
            ViewData["list"] = categories.Where(c => c.ParentId == null);
            return View(category);
        }

        // GET: Admin/Category/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.Parent)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Admin/Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }

        public static string createSLug(string name)
        {
            SlugHelper helper = new SlugHelper();
            string slug = name.Replace("đ", "d").Replace("Đ", "d");
            return helper.GenerateSlug(slug);
        }    
    }
}
