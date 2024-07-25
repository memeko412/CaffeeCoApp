using CaffeeCoApp.Models;
using CaffeeCoApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace CaffeeCoApp.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IWebHostEnvironment environment;
        private readonly int pageSize = 10;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            this.context = context;
            this.environment = environment;
        }

        public IActionResult Index(int pageIndex)
        {
            IQueryable<Product> query = context.Products.OrderByDescending(products => products.Id);

            if (pageIndex < 1) pageIndex = 1;

            decimal productCount = query.Count();
            int totalPages = (int)Math.Ceiling(productCount / pageSize);


            var products = query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

            ViewData["PageIndex"] = pageIndex;
            ViewData["TotalPages"] = totalPages; 
            return View(products);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(ProductDto productDto)
        {
            if (productDto.ImageFile == null) ModelState.AddModelError("ImageFile", "The image is required");

            if (!ModelState.IsValid) return View(productDto);


            // check if the image file has valid extension
            var validExtensions = new[] {".jpg", ".jpeg", ".png"};
            
            string fileExtension = Path.GetExtension(productDto.ImageFile.FileName);

            if(!validExtensions.Contains(fileExtension) ) ModelState.AddModelError("ImageFile", "The image must be a jpg, jpeg or png file");


            // save image file
            string newImageFileName = DateTime.Now.ToString("yyyyMMddHHmmssFFF");
            newImageFileName += Path.GetExtension(fileExtension);

            string fullPath = environment.WebRootPath + "/products/" + newImageFileName;
            using (var stream = System.IO.File.Create(fullPath)) 
            { 
                productDto.ImageFile.CopyTo(stream); 
            }

            Product product = new Product()
            {
                Name = productDto.Name,
                Brand = productDto.Brand,
                Category = productDto.Category,
                Price = productDto.Price,
                Stock = productDto.Stock,
                ImageFileName = newImageFileName,
                Description = productDto.Description,
                CreatedAt = DateTime.Now
            };

            context.Products.Add(product);
            context.SaveChanges();

        return RedirectToAction("Index", "Products");

        }


        public IActionResult Edit(int id)
        {
            var product = context.Products.Find(id);

            if (product == null)
            {
                return RedirectToAction("Index", "Products");
            }

            var productDto = new ProductDto()
            {
                Name = product.Name,
                Brand = product.Brand,
                Category = product.Category,
                Price = product.Price,
                Stock = product.Stock,
                Description = product.Description
            };

            ViewData["ProductId"] = product.Id;
            ViewData["ImageFileName"] = product.ImageFileName;
            ViewData["CreatedAt"] = product.CreatedAt.ToString("MM/dd/yyyy");

            return View(productDto);
        }

        [HttpPost]
        public IActionResult Edit(int id, ProductDto productDto)
        {
            var product = context.Products.Find(id);

            if (product == null) return RedirectToAction("Index", "Products");


            // check if the image file has valid extension
            if (productDto.ImageFile != null)
            {
                var validExtensions = new[] { ".jpg", ".jpeg", ".png" };

                string fileExtension = Path.GetExtension(productDto.ImageFile.FileName);

                if (!validExtensions.Contains(fileExtension)) ModelState.AddModelError("ImageFile", "The image must be a jpg, jpeg or png file");
            }

            if (!ModelState.IsValid) 
            {
                ViewData["ProductId"] = product.Id;
                ViewData["ImageFileName"] = product.ImageFileName;
                ViewData["CreatedAt"] = product.CreatedAt.ToString("MM/dd/yyyy");
                return View(productDto);
            }

            string newImageFileName = product.ImageFileName;

            // update image file if there is one
            if (productDto.ImageFile != null)
            {
                var validExtensions = new[] { ".jpg", ".jpeg", ".png" };

                string fileExtension = Path.GetExtension(productDto.ImageFile.FileName);

                newImageFileName = DateTime.Now.ToString("yyyyMMddHHmmssFFF");
                newImageFileName += fileExtension;

                string fullPath = environment.WebRootPath + "/products/" + newImageFileName;
                using (var stream = System.IO.File.Create(fullPath))
                {
                    productDto.ImageFile.CopyTo(stream);
                }

                // delete old image file
                string oldImagePath = environment.WebRootPath + "/products/" + product.ImageFileName;
                System.IO.File.Delete(oldImagePath);
            }


            // update product in database
            product.Name = productDto.Name;
            product.Brand = productDto.Brand;   
            product.Category = productDto.Category;
            product.Price = productDto.Price;
            product.Stock = productDto.Stock;
            product.Description = productDto.Description;
            product.ImageFileName = newImageFileName;


            context.SaveChanges();

            return RedirectToAction("Index", "Products");


        }

        public IActionResult Delete(int id)
        {
            var product = context.Products.Find(id);
            if(product == null) return RedirectToAction("Index", "Products");

            string imagePath = environment.WebRootPath + "/products/" + product.ImageFileName;
            System.IO.File.Delete(imagePath);

            context.Products.Remove(product);
            context.SaveChanges();

            return RedirectToAction("Index", "Products");
        }

    }
}
