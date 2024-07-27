using CaffeeCoApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CaffeeCoApp.Controllers
{
    [Authorize(Roles="admin"),Route("/Admin/[controller]/{action=Index}/{id?}")]
    public class UsersController : Controller
    {
        private readonly UserManager<AppUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly int pagesize = 10;

        public UsersController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        public IActionResult Index(int? pageIndex)
        {
            IQueryable<AppUser> query = userManager.Users;
            if (pageIndex == null || pageIndex < 1) pageIndex = 1;
            int pageTotal = (int)Math.Ceiling(query.Count() / (double)pagesize);
            query = query.Skip((pageIndex.Value - 1) * pagesize).Take(pagesize);
            var users = query.ToList();

            ViewBag.PageIndex = pageIndex;
            ViewBag.PageTotal = pageTotal;

            return View(users);
        }

        public async Task<IActionResult> Details(string? id)
        {
            if (id == null) return RedirectToAction("Index", "Users");
            var appUser = await userManager.FindByIdAsync(id);
            if (appUser == null) return RedirectToAction("Index", "Users");

            ViewBag.Roles = await userManager.GetRolesAsync(appUser);

            var roles = roleManager.Roles.ToList();
            var roleList = new List<SelectListItem>();
            foreach (var role in roles)
            {
                roleList.Add(
                    new SelectListItem 
                    {
                        Text = role.NormalizedName,
                        Value = role.Name,
                        Selected = await userManager.IsInRoleAsync(appUser, role.Name!)
                    });
            }

            ViewBag.RoleList = roleList;

            return View(appUser);

        }

        public async Task<IActionResult> EditRole(string? id, string? newRole)
        {

            if (id == null || newRole == null)
            {
                // Console.WriteLine("Invalid parameters");
                TempData["Error"] = "Invalid parameters";
                return RedirectToAction("Index", "Users");
            }

            var appUser = userManager.FindByIdAsync(id).Result;
            var validRole = await roleManager.RoleExistsAsync(newRole);
            if (appUser == null || !validRole)
            {
                // Console.WriteLine("User or role not found");
                TempData["Error"] = "User or role not found";
                return RedirectToAction("Index", "Users");
            }

            var currentUser = await userManager.GetUserAsync(User);
            if (currentUser!.Id == appUser.Id){
                // Console.WriteLine("Cannot change your own role");
                TempData["Error"] = "Cannot change your own role";
                return RedirectToAction("Details", "Users", new { id }); 
            }

            var userRoles = await userManager.GetRolesAsync(appUser);
            
            await userManager.RemoveFromRolesAsync(appUser, userRoles);
            await userManager.AddToRoleAsync(appUser, newRole);
            TempData["Success"] = "Role updated successfully";
            return RedirectToAction("Details", "Users", new { id });
        }

    }
}
