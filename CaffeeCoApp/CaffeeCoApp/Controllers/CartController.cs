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

        public CartController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        public IActionResult Index()
        {
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

            return RedirectToAction("Confirm");


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
