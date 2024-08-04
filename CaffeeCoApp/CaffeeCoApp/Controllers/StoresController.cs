using CaffeeCoApp.Models;
using CaffeeCoApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CaffeeCoApp.Controllers
{
    [Authorize(Roles = "admin"), Route("/Admin/[controller]/{action=Index}/{id?}")]
    public class StoresController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly int pagesize = 10;

        public StoresController(ApplicationDbContext context)
        {
            this.context = context;
        }
        public IActionResult Index(int? pageIndex, string? search, string? column, string? orderBy)
        {
            IQueryable<Store> query = context.Stores;
            // search stores
            if (search != null) query = query.Where(store => store.Name.ToLower().Contains(search.ToLower()));

            if (pageIndex == null || pageIndex < 1) pageIndex = 1;
            int pageTotal = (int)Math.Ceiling(query.Count() / (double)pagesize);
            query = query.Skip((pageIndex.Value - 1) * pagesize).Take(pagesize);
            var stores = query.ToList();
            ViewBag.PageIndex = pageIndex;
            ViewBag.PageTotal = pageTotal;
            ViewData["Column"] = column;
            ViewData["OrderBy"] = orderBy;
            ViewData["Search"] = search ?? "";
            return View(stores);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Store store)
        {
            if (!ModelState.IsValid) return View(store);

            context.Stores.Add(store);
            context.SaveChanges();


            return RedirectToAction("Index");
        }

        public IActionResult Details(int? id)
        {
            var store = context.Stores.Find(id);
            return View(store);
        }

        public IActionResult Delete(int? id) 
        {
            if (id == null) {
                TempData["Error"] = "Invalid parameters";
                return RedirectToAction("Index", "Stores");
            } 
            var store = context.Stores.Find(id);
            if (store == null) {
                TempData["Error"] = "Store not found";
                return RedirectToAction("Index","Stores");
            }
            context.Stores.Remove(store);
            context.SaveChanges();
            TempData["Success"] = "Store deleted successfully";
            return RedirectToAction("Index", "Stores");
        }
    }
}
