using CaffeeCoApp.Models;
using CaffeeCoApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CaffeeCoApp.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<AppUser> userManager;
        private readonly IConfiguration configuration;

        public CartController(ApplicationDbContext context, UserManager<AppUser> userManager, IConfiguration configuration)
        {
            this.context = context;
            this.userManager = userManager;
            this.configuration = configuration;
        }

        public IActionResult Index()
        {
            var apiKey = configuration["GoogleMaps:ApiKey"];
            ViewData["GoogleMapsKey"] = apiKey;
            List<OrderItem> cartItems = CartHelper.GetCartItems(Request, Response, context);
            decimal subtotal = CartHelper.GetSubTotal(cartItems);
            List<Store> stores = context.Stores.ToList();
            ViewBag.CartItems = cartItems;
            ViewBag.Subtotal = subtotal;
            ViewBag.Stores = stores;
            CheckoutDto checkoutDto = new CheckoutDto();

            return View(checkoutDto);
        }

        [Authorize]
        [HttpPost]
        public IActionResult Index(CheckoutDto checkoutDto)
        {
            var apiKey = configuration["GoogleMaps:ApiKey"];
            ViewData["GoogleMapsKey"] = apiKey;
            List<OrderItem> cartItems = CartHelper.GetCartItems(Request, Response, context);
            decimal subtotal = CartHelper.GetSubTotal(cartItems);
            List<Store> stores = context.Stores.ToList();
            ViewBag.CartItems = cartItems;
            ViewBag.Subtotal = subtotal;
            ViewBag.Stores = stores;
            if (!ModelState.IsValid)
            {
                string errorMessage = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                ViewBag.ErrorMsg = errorMessage;
                Console.WriteLine(errorMessage);
                return View(checkoutDto);
            }

            if (cartItems.Count == 0)
            {
                ViewBag.ErrorMsg = "Your cart is empty";
                return View(checkoutDto);
            }

            TempData["DeliveryAddress"] = checkoutDto.ShippingAddress;
            TempData["IsPickUp"] = checkoutDto.IsPickUp;
            TempData["DeliveryDate"] = checkoutDto.DeliveryDate;
            TempData["StoreId"] = checkoutDto.StoreId;

            return RedirectToAction("Confirm");
        }

        public IActionResult Confirm()
        {
            var apiKey = configuration["GoogleMaps:ApiKey"];
            ViewData["GoogleMapsKey"] = apiKey;
            List<OrderItem> cartItems = CartHelper.GetCartItems(Request, Response, context);
            decimal total = CartHelper.GetSubTotal(cartItems);
            int cartSize = 0;
            foreach (var item in cartItems)
            {
                cartSize += item.Quantity;
            }
            string deliveryAddress = TempData["DeliveryAddress"] as string ?? "";
            bool isPickUp = (bool)(TempData["IsPickUp"] ?? false);
            DateTime deliveryDate = (DateTime)(TempData["DeliveryDate"]!);
            int storeId = (int)TempData["StoreId"];
            TempData.Keep();

            if (cartSize == 0 || deliveryAddress.Length == 0)
            {
                Console.WriteLine("Cart is empty or delivery address is empty");
                return RedirectToAction("Index");
            }

            if (isPickUp && !IsStoreAvailableForPickup(storeId, deliveryDate))
            {
                TempData["Error"] = "Store is not available for pickup on the day you selected!";
                Console.WriteLine("Store is not available for pickup");
                return RedirectToAction("Index");
            }

            if (!isPickUp && !IsValidDeliveryDate(deliveryDate))
            {
                TempData["Error"] = "Delivery is not available on the day you selected!";
                Console.WriteLine("Invalid delivery date");
                return RedirectToAction("Index");
            }

            ViewBag.DeliveryAddress = deliveryAddress;
            ViewBag.IsPickUp = isPickUp;
            ViewBag.DeliveryDate = deliveryDate;
            ViewBag.StoreId = storeId;


            return View();
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Confirm(int? a)
        {
            var apiKey = configuration["GoogleMaps:ApiKey"];
            ViewData["GoogleMapsKey"] = apiKey;
            var cartItems = CartHelper.GetCartItems(Request, Response, context);

            string deliveryAddress = TempData["DeliveryAddress"] as string ?? "";
            bool isPickUp = (bool)(TempData["IsPickUp"] ?? false);
            DateTime deliveryDate = (DateTime)(TempData["DeliveryDate"]!);
            int storeId = (int)TempData["StoreId"];
            TempData.Keep();
            if (cartItems.Count == 0 || deliveryAddress.Length == 0)
            {
                Console.WriteLine("Cart is empty or delivery address is empty");
                return RedirectToAction("Index", "Home");
            }

            if (isPickUp && !IsStoreAvailableForPickup(storeId, deliveryDate))
            {
                Console.WriteLine("Store is not available for pickup");
                return RedirectToAction("Index", "Home");
            }

            if (!isPickUp && !IsValidDeliveryDate(deliveryDate))
            {
                Console.WriteLine("Store is not available for pickup");
                return RedirectToAction("Index", "Home");
            }

            var appUser = await userManager.GetUserAsync(User);
            if (appUser == null) 
            {
                Console.WriteLine("Invalid User!");
                return RedirectToAction("Index", "Home");
            }

            var order = new Order
            {
                Client = appUser!,
                ClientId = appUser!.Id,
                Items = cartItems,
                IsPickUp = isPickUp,
                ShippingAddress = deliveryAddress,
                DeliveryDate = deliveryDate,
                CreatedAt = DateTime.Now,
                Store = context.Stores.Find(storeId),
                ShippingStatus = "pending"
            };
            context.Orders.Add(order);
            context.SaveChanges();

            foreach (OrderItem item in cartItems)
            {
                var product = context.Products.FirstOrDefault(p => p.Id == item.Product.Id);
                if (product != null)
                {
                    Console.WriteLine("Updating stock for product: " + product.Name);
                    Console.WriteLine("Old Stock: " + product.Stock);
                    Console.WriteLine("In Cart: " + item.Quantity);
                    product.Stock -= item.Quantity;
                    Console.WriteLine("New Stock: " + product.Stock);
                    context.Products.Update(product);
                    context.SaveChanges();
                }
            }




            Response.Cookies.Delete("shopping_cart");
            ViewBag.SuccessMessage = "Order Created Successfully";

            return View();
        }


        private bool IsStoreAvailableForPickup(int storeId, DateTime selectedDate)
        {
            var dailyPickupLimit = context.Stores.Where(s => s.Id == storeId).Select(s => s.DailyPickUpLimit).FirstOrDefault();

            var pickupCount = context.Orders
                .Where(o => o.IsPickUp && o.DeliveryDate.Date == selectedDate.Date && o.Store.Id == storeId)
                .Count();

            return pickupCount < dailyPickupLimit;
        }

        private bool IsValidDeliveryDate(DateTime selectedDate)
        {
            var dayOfWeek = selectedDate.DayOfWeek;
            return dayOfWeek != DayOfWeek.Saturday && dayOfWeek != DayOfWeek.Sunday;
        }
    }
}
