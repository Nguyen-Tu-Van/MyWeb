using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyWeb.Models;
using X.PagedList;

namespace MyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "OrderOnly")]
    public class OrderController : Controller
    {
        private MainDbContext _context;
        public OrderController(MainDbContext context)
        {
            _context = context;
        }
        // GET: OrderController
        public ActionResult Index(int? page)
        {
            var kq1 = getCountOrderByStatus(1,_context);
            var kq2 = getCountOrderByStatus(2, _context);
            var kq3 = getCountOrderByStatus(3, _context);
            var kq4 = getCountOrderByStatus(4, _context);
            var total = (from order in _context.Orders
                        where order.Payment == 1
                        group order by order.Payment into g
                        select new {total = g.Sum(p => p.Total)}).FirstOrDefault();
            int[] result = { kq1, kq2, kq3, kq4 };
            ViewData["result"] = result;
            try
            {
                ViewData["total"] = total.total;
            }catch (Exception ex) { ViewData["total"] = 0; }

            var orders = _context.Orders.ToList().ToPagedList(page ?? 1, 4);
            return View(orders);
        }
        // GET: OrderController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: OrderController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: OrderController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: OrderController/Edit/5
        public ActionResult Edit(int id)
        {
            var order = _context.Orders.Include(u => u.user).Include(o=>o.OrderDetails).ThenInclude(o=>o.product).Where(o => o.Id == id).FirstOrDefault();
            if (order == null)
            {
                return NotFound("Không tìm thấy đơn hàng");
            }
            return View(order);
        }

        // POST: OrderController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            var order = _context.Orders.Find(id);
            if(order == null)
            {
                return NotFound();
            }
            order.Payment = Int32.Parse(Request.Form["Payment"]);
            order.Status = Int32.Parse(Request.Form["Status"]);
            order.updatedAt = DateTime.Now;
            _context.SaveChanges();
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: OrderController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: OrderController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        public static int getCountOrderByStatus(int status, MainDbContext _context)
        {
            var result = from order in _context.Orders
                         where order.Status == status
                         select order;
            return result.ToList().Count();
        }
    }
}
