using System.ComponentModel.DataAnnotations;

namespace CaffeeCoApp.Models
{
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Current Password is required."), MaxLength(20, ErrorMessage = "The password should not exceed 20 characters.")]
        public string CurrentPassword { get; set; } = "";

        [Required(ErrorMessage = "New Password Field is required"),
            RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{10,}$", 
            ErrorMessage = "The Password must be at least 10 characters long, have capital" +
            " and non-capital letters, and contain non-alphanumeric characters."),
            MaxLength(20, ErrorMessage = "The password should not exceed 20 characters.")]
        public string NewPassword { get; set; } = "";
        [Required(ErrorMessage = "Confirm Password Field is required"), Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = "";

    }
}
