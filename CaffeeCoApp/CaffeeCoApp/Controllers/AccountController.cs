using CaffeeCoApp.Models;
using CaffeeCoApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CaffeeCoApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly IConfiguration configuration;

        public AccountController(UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.configuration = configuration;
        }
        public IActionResult Register()
        {
            if (signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            if (signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }
            if (!ModelState.IsValid) 
            { 
                return View(registerDto); 
            }

            var user = new AppUser()
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                UserName = registerDto.Email,
                Email = registerDto.Email,
                PhoneNumber = registerDto.PhoneNumber,
                Address = registerDto.Address,
                CreatedAt = DateTime.Now,
            };

            var result = await userManager.CreateAsync(user, registerDto.Password);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "client");

                await signInManager.SignInAsync(user, isPersistent: false);

                return RedirectToAction("Index", "Home");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(registerDto);
        }

        public async Task<IActionResult> Logout()
        {
            if (signInManager.IsSignedIn(User)){
                await signInManager.SignOutAsync();
            }
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Login()
        {
            if (signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            if (signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }
            if (!ModelState.IsValid) return View(loginDto);
            var result = await signInManager.PasswordSignInAsync(loginDto.Email, loginDto.Password, loginDto.RememberMe, false);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.ErrorMsg = "Login Failed! ";
            }
            
            return View(loginDto);
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var appUser = await userManager.GetUserAsync(User);
            var profileDto = new ProfileDto()
            {
                FirstName = appUser!.FirstName,
                LastName = appUser!.LastName,
                Email = appUser!.Email ?? "",
                PhoneNumber = appUser!.PhoneNumber,
                Address = appUser!.Address,
            };

            return View(profileDto);
        }

        [Authorize, HttpPost]
        public async Task<IActionResult> Profile(ProfileDto profileDto) 
        {
            if (!ModelState.IsValid) {
                ViewBag.ErrorMsg = "Please fill all required fields with valid inputs!";
                return View(profileDto);
            } 

            var appUser = await userManager.GetUserAsync(User);
            if (appUser == null) return RedirectToAction("Index","Home");

            // Check if the email already exists in the database
            var existingUser = await userManager.FindByEmailAsync(profileDto.Email);
            if (existingUser != null && existingUser.Id != appUser.Id)
            {
                ViewBag.ErrorMsg = "Email already exists!";
                return View(profileDto);
            }

            appUser.FirstName = profileDto.FirstName;
            appUser.LastName = profileDto.LastName; 
            appUser.PhoneNumber = profileDto.PhoneNumber;
            appUser.Address = profileDto.Address;
            appUser.Email = profileDto.Email;
            appUser.UserName = profileDto.Email;

            var result = await userManager.UpdateAsync(appUser);

            if (!result.Succeeded) 
            { 
                ViewBag.ErrorMsg = "Failed to update profile!" + result.Errors.First().Description;
            }
            else
            {
                ViewBag.SuccessMsg = "Profile updated successfully!";
            }

            return View(profileDto);
        }

        [Authorize]
        public IActionResult ChangePassword() 
        {
            return View();
        }

        [Authorize, HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            if(!ModelState.IsValid) return View();

            var appUser = await userManager.GetUserAsync(User);
            if (appUser == null) return RedirectToAction("Index", "Home");

            var result = await userManager.ChangePasswordAsync(appUser, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
            if (result.Succeeded) 
            { 
                ViewBag.SuccessMsg = "Password changed successfully!";
            }
            else
            {
                ViewBag.ErrorMsg = "Failed to change password!" + result.Errors.First().Description;
            }

            return View();
        }

        public IActionResult ForgotPassword()
        {
            if (signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword([Required, EmailAddress] string email)
        {
            if (signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Email = email;

            if (!ModelState.IsValid)
            {
                ViewBag.EmailError = ModelState["email"]?.Errors.First().ErrorMessage ?? "Invalid Email Address";
                return View();
            }
            // Check if the email exists in the database
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ViewBag.ErrorMsg = "Account associated with this email does not exist!";
            } else
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                string resetUrl = Url.ActionLink("ResetPassword", "Account", new { token }) ?? "URL Error";

                // Console.WriteLine("PW reset link: " + resetUrl);

                string sendername = configuration["EmailSettings:SenderName"] ?? "";
                string senderemail = configuration["EmailSettings:SenderEmail"] ?? "";
                string username = user.FirstName + " " + user.LastName;
                string subject = "CaffeeCo Account Password Reset";
                string message = "Dear " + username + ",\n\n" +
                    "Please click the link below to reset your password:\n" +
                    resetUrl + "\n\n" +
                    "If you did not request a password reset, please ignore this email.\n\n" +
                    "Best Regards,\nCaffeeCo Team";

                EmailService.SendEmail(sendername, senderemail, email, username, message, subject);

                ViewBag.SuccessMsg = "Password reset link sent to your email!";
            }

            return View();
        }

        public IActionResult ResetPassword(string? token)
        {
            if (signInManager.IsSignedIn(User) || token == null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }


        [Authorize]
        public IActionResult AccessDenied()
        {
            return RedirectToAction("Index", "Home");
        }

    }
}
