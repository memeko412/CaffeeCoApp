using CaffeeCoApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

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

        public async Task<IActionResult> Index(int? pageIndex, string? search, string? column, string? orderBy, string? role)
        {
            IQueryable<AppUser> query = userManager.Users;

            // search users
            if (search != null) query = query.Where(user => (user.FirstName + " " + user.LastName).Contains(search.ToLower()));
            if (role != null && role.Length > 0)
            {
                var roleUsers = await userManager.GetUsersInRoleAsync(role);
                query = query.Where(user => roleUsers.Contains(user));
            }

            string[] validCols = { "Name", "Email", "CreatedAt" };
            string[] validOrderBys = { "desc", "asc" };

            if (column == "Name")
            {
                if (orderBy == "asc")
                {
                    query = query.OrderBy(user => (user.FirstName + " " + user.LastName));
                }
                else
                {
                    query = query.OrderByDescending(user => (user.FirstName + " " + user.LastName));
                }
            }
            else if (column == "Email")
            {
                if (orderBy == "asc")
                {
                    query = query.OrderBy(user => user.Email);
                }
                else
                {
                    query = query.OrderByDescending(user => user.Email);
                }
            }
            else if (column == "CreatedAt")
            {
                if (orderBy == "asc")
                {
                    query = query.OrderBy(user => user.CreatedAt);
                }
                else
                {
                    query = query.OrderByDescending(user => user.CreatedAt);
                }
            }

            if (pageIndex == null || pageIndex < 1) pageIndex = 1;
            int pageTotal = (int)Math.Ceiling(query.Count() / (double)pagesize);
            query = query.Skip((pageIndex.Value - 1) * pagesize).Take(pagesize);
            var users = query.ToList();

            ViewData["PageIndex"] = pageIndex;
            ViewData["TotalPages"] = pageTotal;
            ViewData["Column"] = column;
            ViewData["Role"] = role;
            ViewData["OrderBy"] = orderBy;
            ViewData["Search"] = search ?? "";

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
            
            var removeRoleResult = await userManager.RemoveFromRolesAsync(appUser, userRoles);
            var changeRoleResult = await userManager.AddToRoleAsync(appUser, newRole);
            if (removeRoleResult.Succeeded && changeRoleResult.Succeeded)
            {
                TempData["Success"] = "Role updated successfully";
                return RedirectToAction("Details", "Users", new { id });
            }

            TempData["Error"] = "Something went wrong while update role for this account: " + removeRoleResult.Errors.First().Description + "/n" + changeRoleResult.Errors.First().Description;
            return RedirectToAction("Details", "Users", new { id });
        }

        public async Task<IActionResult> DeleteAccount(string? id)
        {
            if (id == null)
            {
                TempData["Error"] = "Invalid parameters";
                return RedirectToAction("Index", "Users");
            }

            var appUser = userManager.FindByIdAsync(id).Result;
            if (appUser == null)
            {
                TempData["Error"] = "User not found";
                return RedirectToAction("Index", "Users");
            }

            var currentUser = await userManager.GetUserAsync(User);
            if (currentUser!.Id == appUser.Id)
            {
                TempData["Error"] = "Cannot delete your own account!";
                return RedirectToAction("Details", "Users", new { id });
            }

            var result = await userManager.DeleteAsync(appUser);
            if (result.Succeeded)
            {
                TempData["Success"] = "Account deleted successfully";
                return RedirectToAction("Index", "Users");
            }
            TempData["Error"] = "Something went wrong while deleting this account: " + result.Errors.First().Description;
            return RedirectToAction("Details", "Users", new { id });

        }

    }
}
