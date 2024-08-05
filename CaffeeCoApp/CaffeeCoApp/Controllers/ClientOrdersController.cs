﻿using CaffeeCoApp.Models;
using CaffeeCoApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CaffeeCoApp.Controllers
{
    [Authorize(Roles = "client")]
    [Route("/Client/Orders/{action=Index}/{id?}")]
    public class ClientOrdersController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<AppUser> userManager;
        private readonly int pageSize = 10;

        public ClientOrdersController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }
        public async Task<IActionResult> Index(int pageIndex)
        {
            var currentUser = await userManager.GetUserAsync(User);
            if (currentUser == null) 
            {
                return RedirectToAction("Index", "Home");
            }

            IQueryable<Order> query = context.Orders
                .Include(o => o.Items).OrderByDescending(o => o.Id).Where(o => o.ClientId == currentUser.Id);

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

        public async Task<IActionResult> Detail(int id)
        {
            var currentUser = await userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var order = context.Orders.Include(o => o.Items).ThenInclude(oi => oi.Product).Where(o => o.ClientId == currentUser.Id).FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return RedirectToAction("Index");
            }

            return View(order);
        }
    }


}
