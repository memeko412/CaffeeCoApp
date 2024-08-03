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

        public StoresController(ApplicationDbContext context)
        {
            this.context = context;
        }
        public IActionResult Index()
        {
            IQueryable<Store> query = context.Stores;
            var stores = query.ToList();
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
    }
}
