using CaffeeCoApp.Models;
using CaffeeCoApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CaffeeCoApp.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("/Admin/Orders/{action=Index}/{id?}")]
    public class AdminOrdersController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly int pageSize = 10;

        public AdminOrdersController(ApplicationDbContext context)
        {
            this.context = context;
        }
        public IActionResult Index(int pageIndex)
        {
            IQueryable<Order> query = context.Orders.Include(o => o.Client)
                .Include(o => o.Items).OrderByDescending(o => o.Id);

            if (pageIndex <= 0)
            {
                pageIndex = 1;
            }

            decimal count = query.Count();
            int totalPages = (int)Math.Ceiling(count / pageSize);

            query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);

            var orders = query.ToList();


            ViewBag.PageIndex = pageIndex;
            ViewBag.TotalPages = totalPages;
            ViewBag.Orders = orders;
            return View();
        }

        public IActionResult Detail(int id) 
        { 
            var order = context.Orders.Include(o => o.Client).Include(o => o.Items).ThenInclude(i => i.Product).FirstOrDefault(o => o.Id == id); 

            if (order == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.NumOrders = context.Orders.Where(o => o.ClientId == order.ClientId).Count();
            return View(order);
        }
    }
}
