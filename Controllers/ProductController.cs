using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyWeb.Models;
using X.PagedList;
namespace MyWeb.Controllers
{
    public class ProductController : Controller
    {
        private MainDbContext _context;
        private CartService _cartService;
        private UserManager<User> _userManager;
        public ProductController(MainDbContext context, CartService cartService, UserManager<User> userManager)
        {
            _context = context;
            _cartService = cartService;
            _userManager = userManager;
        }

        public IActionResult Index(string? slug,int? page)
        {
            var products = _context.Products.OrderByDescending(p=>p.Id).ToPagedList(page ?? 1, 6); 
            var listCategory = _context.Categories.Include(c => c.Parent).Include(c => c.ChildCategories).ToList();
            var categories = listCategory.Where(c => c.ParentId == null).ToList();
            dynamic category;
            if (Request.RouteValues["slug"] != null)
            {
                category = listCategory.Where(c => c.Slug == slug).FirstOrDefault();
            }
            else category = null;
            ViewData["key"] = "";
            if (category!=null)
            {
                //Lấy tất cả ID của danh mục
                List<int> listCategoryId = getCategoryId(slug);
                products = _context.Products.Where(c => listCategoryId.Contains(c.CategoryId)).ToPagedList(page ?? 1, 6);
                ViewData["category"] = category;
            }
            if(Request.Query["key"].ToString() != "")
            {
                products = _context.Products.Where(p=>p.Name.Contains(Request.Query["key"])).OrderByDescending(p => p.Id).ToPagedList(page ?? 1, 6);
                ViewData["key"] = Request.Query["key"].ToString();
            }    

            ViewData["products"] = products;
            ViewData["categories"] = categories;

            return View();
        }

        public List<int> getCategoryId(string slug)
        {
            var listCategory = _context.Categories.Include(c => c.ChildCategories).ToList();
            var category = listCategory.Where(c => c.Slug == slug).FirstOrDefault();
            List<int> listCategoryId = new List<int>();
            dequyCategoryId(listCategoryId, category);
            return listCategoryId;
        }
        public void dequyCategoryId(List<int>a, Category category)
        {
            a.Add(category.Id);
            if(category.ChildCategories.Count > 0)
            {
                category.ChildCategories.ToList().ForEach(p => {
                    dequyCategoryId(a, p);
                });
            }
        }

        public IActionResult Detail(string slug)
        {
            var product = _context.Products.Where(p=>p.Slug == slug).Include(p=>p.productPhotos).FirstOrDefault();
            if(product==null) return NotFound("Khong tim thay san pham");
            var listCategory = _context.Categories.Include(c => c.Parent).ToList();
            var category = listCategory.Where(c => c.Id == product.CategoryId).FirstOrDefault(); 
            ViewData["category"] = category;
            ViewData["product"] = product;

            return View();
        }

        /// Thêm sản phẩm vào cart
        [Route("addcart/{productid:int}", Name = "addcart")]
        public IActionResult AddToCart([FromRoute] int productid)
        {
            var product = _context.Products
                .Where(p => p.Id == productid)
                .FirstOrDefault();
            if (product == null)
                return NotFound("Không có sản phẩm");

            // Xử lý đưa vào Cart ...
            var cart = _cartService.GetCartItems();
            var cartitem = cart.Find(p => p.product.Id == productid);
            if (cartitem != null)
            {
                // Đã tồn tại, tăng thêm 1
                cartitem.quantity++;
            }
            else
            {
                //  Thêm mới
                cart.Add(new CartItem() { quantity = 1, product = product });
            }
            // Lưu cart vào Session
            _cartService.SaveCartSession(cart);
            // Chuyển đến trang hiện thị Cart
            return RedirectToAction(nameof(Cart));
        }

        [Route("/cart", Name = "cart")]
        public IActionResult Cart()
        {
            return View(_cartService.GetCartItems());
        }

        // Cập nhật giỏ hàng
        [Route("/updatecart", Name = "updatecart")]
        [HttpPost]
        public IActionResult UpdateCart([FromForm] int productid, [FromForm] int quantity)
        {
            // Cập nhật Cart thay đổi số lượng quantity ...
            var cart = _cartService.GetCartItems();
            var cartitem = cart.Find(p => p.product.Id == productid);
            if (cartitem != null)
            {
                cartitem.quantity = quantity;
            }
            if (quantity<=0)
            {
                cartitem.quantity = 1;
            }
            _cartService.SaveCartSession(cart);
            // Trả về mã thành công (không có nội dung gì - chỉ để Ajax gọi)
            return Ok();
        }

        /// xóa item trong cart
        [Route("/removecart/{productid:int}", Name = "removecart")]
        public IActionResult RemoveCart([FromRoute] int productid)
        {
            var cart = _cartService.GetCartItems();
            var cartitem = cart.Find(p => p.product.Id == productid);
            if (cartitem != null)
            {
                cart.Remove(cartitem);
            }

            _cartService.SaveCartSession(cart);
            return RedirectToAction(nameof(Cart));
        }

        [HttpPost]
        [Route("/checkout", Name ="checkout")]
        public IActionResult CheckOut()
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var listCart = _cartService.GetCartItems();
                var total = 0;
                listCart.ForEach(item =>
                {
                    total += (int)item.product.Price * item.quantity;
                });
                var order = new Order
                {
                    Total = total,
                    Address = Request.Form["Address"],
                    Payment = 0,
                    Status = 1,
                    Content = Request.Form["Content"],
                    createdAt = DateTime.Now,
                    updatedAt = DateTime.Now,
                    UserId = _userManager.GetUserId(User)
                };
                _context.Orders.Add(order);
                _context.SaveChanges();

                var listOrderDetail = new List<OrderDetail>();
                listCart.ForEach(item =>
                {
                    listOrderDetail.Add(new OrderDetail { Quantity = item.quantity, Price = item.product.Price, OrderId = order.Id, ProductId = item.product.Id });
                });
                _context.OrderDetails.AddRange(listOrderDetail);
                _context.SaveChanges();
                transaction.Commit();
                TempData["success"] = $"Đặt hàng thành công";
            }catch(Exception e)
            {
                transaction.Rollback();
                TempData["success"] = $"Error : Có lỗi xảy ra. Đơn hàng chưa được tạo";
            }
            _cartService.ClearCart();
            return RedirectToRoute("cart");
        }
    }
}
