#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyWeb.Models;
using Slugify;
using X.PagedList;

namespace MyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "ProductOnly")]
    public class ProductController : Controller
    {
        private readonly MainDbContext _context;
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;
        [TempData]
        public string StaticMessage { get; set; }

        public ProductController(MainDbContext context,Microsoft.AspNetCore.Hosting.IHostingEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Admin/Product
        public async Task<ViewResult> Index(int? page)
        {
            ViewData["key"] = "";
            ViewData["count"] = _context.Products.Count();

            dynamic mainDbContext;
            if (Request.Query["key"].ToString() != "")
            {
                ViewData["key"] = Request.Query["key"].ToString();
                ViewData["count"] = _context.Products.Where(p => p.Name.Contains(Request.Query["key"])).Count();
                mainDbContext = _context.Products.Where(p => p.Name.Contains(Request.Query["key"])).OrderByDescending(p => p.Id).Include(p => p.category).ToPagedList(page ?? 1, 4);
            }    
            else mainDbContext = _context.Products.OrderByDescending(p => p.Id).Include(p => p.category).ToPagedList(page ?? 1, 4);

            return View(mainDbContext);
        }

        // GET: Admin/Product/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Admin/Product/Create
        public IActionResult Create()
        {
            //ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            var categories = _context.Categories.Include(c=>c.Parent).Include(c=>c.ChildCategories).ToList();
            ViewData["list"] = categories.Where(c => c.ParentId == null);

            return View();
        }

        // POST: Admin/Product/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Slug,Image,Price,Limit,Status,CategoryId,Content")] Product product)
        {
            var imageNew = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds()+".jpg";
            if (ModelState.IsValid)
            {
                try
                {
                    var fileRequest = Request.Form.Files["Image"];
                    var file = Path.Combine(_environment.ContentRootPath, "wwwroot/images", imageNew);
                    var fileStream = new FileStream(file, FileMode.Create);
                    await fileRequest.CopyToAsync(fileStream);
                    product.Image = imageNew;
                }
                catch (Exception ex)
                {
                    product.Image = "default.jpg";
                }
                product.Slug = createSLug(product.Name);
                _context.Add(product);
                await _context.SaveChangesAsync();
                TempData["success"] = $"Tạo thành công sản phẩm {product.Name}";

                return RedirectToAction(nameof(Index));
            }
            else
            {
                Console.WriteLine("--------------------------------------");
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                errors.ToList().ForEach(x => Console.WriteLine(x.ErrorMessage));
                Console.WriteLine("--------------------------------------");
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Admin/Product/Edit/5
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
            //iewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            var categories = _context.Categories.Include(c => c.Parent).Include(c => c.ChildCategories).ToList();
            ViewData["list"] = categories.Where(c => c.ParentId == null);
            //ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Admin/Product/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Slug,Image,Price,Limit,Status,CategoryId,Content")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }
            var imageNew = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds() + ".jpg";
            if (ModelState.IsValid)
            {
                try
                {
                    try
                    {
                        var fileRequest = Request.Form.Files[0];
                        var file = Path.Combine(_environment.ContentRootPath, "wwwroot/images", imageNew);
                        var fileStream = new FileStream(file, FileMode.Create);
                        fileRequest.CopyTo(fileStream);
                        product.Image = imageNew;
                    }
                    catch (Exception ex)
                    {
                        
                    }
                    product.Slug = createSLug(product.Name);
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    TempData["success"] = $"Cập nhật thành công sản phẩm {product.Name}";
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
            else
            {
                Console.WriteLine("--------------------------------------");
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                errors.ToList().ForEach(x => Console.WriteLine(x.ErrorMessage));
                Console.WriteLine("--------------------------------------");
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Admin/Product/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Admin/Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            TempData["success"] = $"Xóa thành công sản phẩm {product.Name}";

            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

        public class UploadOneFile
        {
            [Required(ErrorMessage = "Bạn chưa chọn file")]
            [DataType(DataType.Upload)]
            [FileExtensions(Extensions = "jpg")]
            [Display(Name = "Chọn file upload")]
            public IFormFile FileUpload { get; set; }
        }    

        [HttpGet]
        public IActionResult UploadPhoto(int id)
        {
            var product = _context.Products.Where(p => p.Id == id).Include(p=>p.productPhotos).FirstOrDefault();
            if(product == null) return NotFound("Không tìm thấy sản phẩm");
            ViewData["product"] = product;
            return View(new UploadOneFile());
        }

        [HttpPost, ActionName("UploadPhoto")]
        public IActionResult UploadPhotoAsync(int id, [Bind ("FileUpload")] UploadOneFile f)
        {
            var product = _context.Products.Where(p => p.Id == id).Include(p => p.productPhotos).FirstOrDefault();
            if (product == null) return NotFound("Không tìm thấy sản phẩm");
            ViewData["product"] = product;

            if(product!=null)
            {
                try
                {
                    var file1 = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + Path.GetExtension(f.FileUpload.FileName);
                    var file = Path.Combine(_environment.ContentRootPath, "wwwroot/images/ProductPhoto", file1);
                    var fileStream = new FileStream(file, FileMode.Create);
                    f.FileUpload.CopyTo(fileStream);
                    _context.Add(new ProductPhoto()
                    {
                        ProductId = id,
                        Image = file1
                    });
                    _context.SaveChanges();
                }catch(Exception ex)
                {
                    return View(new UploadOneFile());
                }
            }    

            return View(new UploadOneFile());
        }

        [HttpPost]
        public IActionResult ListPhoto(int id)
        {
            var product = _context.Products.Where(p => p.Id == id).Include(p=>p.productPhotos).FirstOrDefault();
            if(product==null)
            {
                return Json(
                    new
                    {
                        success = 0,
                        message = "Khong tim thay san pham"
                    });
            }
            var listPhoto = product.productPhotos.Select(p => new { id = p.Id, path = p.Image } );
            return Json(
                    new
                    {
                        success = 1,
                        photo = listPhoto
                    });
        }

        [HttpPost]
        public IActionResult createPhoto(int id, [Bind("FileUpload")] UploadOneFile f)
        {
            var product = _context.Products.Where(p => p.Id == id).Include(p => p.productPhotos).FirstOrDefault();
            if (product == null) return NotFound("Không tìm thấy sản phẩm");

            if (product != null)
            {
                try
                {
                    var file1 = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + Path.GetExtension(f.FileUpload.FileName);
                    var file = Path.Combine(_environment.ContentRootPath, "wwwroot/images/ProductPhoto", file1);
                    var fileStream = new FileStream(file, FileMode.Create);
                    f.FileUpload.CopyTo(fileStream);
                    _context.Add(new ProductPhoto()
                    {
                        ProductId = id,
                        Image = file1
                    });
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    
                }
            }

            return Ok();
        }

        [HttpPost]
        public IActionResult RemovePhoto(int id)
        {
            var photo = _context.ProductPhotos.Where(p => p.Id == id).FirstOrDefault();
            if (photo == null)
            {
                return Json(
                    new
                    {
                        success = 0,
                        message = "Khong tim thay hinh anh san pham"
                    });
            }
            _context.ProductPhotos.Remove(photo);
            _context.SaveChanges();
            return Json(
                    new
                    {
                        success = 1,
                        message = "Xoa hinh anh thanh cong"
                    });
        }
        
        public static string createSLug(string name)
        {
            SlugHelper helper = new SlugHelper();
            string slug = name.Replace("đ", "d").Replace("Đ", "d");
            return helper.GenerateSlug(slug);
        }
    }
}
