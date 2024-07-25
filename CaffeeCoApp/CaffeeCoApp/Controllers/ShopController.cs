﻿using CaffeeCoApp.Models;
using CaffeeCoApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace CaffeeCoApp.Controllers
{
    public class ShopController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly int pageSize = 10;
        public ShopController(ApplicationDbContext context)
        {
            this.context = context;
        }
        public IActionResult Index(int pageIndex, string? search, string? brand, string? category, string? sort)
        {
            IQueryable<Product> query = context.Products;

            
            // Search
            if (search != null && search.Length > 0)
            {
                query = query.Where(p => p.Name.ToLower().Contains(search.ToLower()));
            }

            if (brand != null && brand.Length > 0)
            {
                query = query.Where(p => p.Brand.ToLower().Contains(brand.ToLower()));
            }

            if (category != null && category.Length > 0)
            {
                query = query.Where(p => p.Category == category);
            }

            // Sort
            if (sort == "price_asc")
            {
                query = query.OrderBy(p => p.Price);
            }
            else if (sort == "price_desc")
            {
                query = query.OrderByDescending(p => p.Price);
            }
            else
            {
                query = query.OrderByDescending(p => p.Id);
            }


            // pagination
            if (pageIndex < 1)
            {
                pageIndex = 1;
            }

            decimal count = query.Count();
            int totalPages = (int)Math.Ceiling(count / pageSize);
            query=query.Skip((pageIndex - 1) * pageSize).Take(pageSize);


            var products = query.ToList();

            ViewBag.Products = products;
            ViewBag.PageIndex = pageIndex;
            ViewBag.TotalPages = totalPages;
            
            var shopSearchmodel = new ShopSearchModel()
            {
                Search = search,
                Brand = brand,
                Category = category,
                Sort = sort
            };

            return View(shopSearchmodel);
        }
    }
}
