using System.ComponentModel.DataAnnotations;

namespace CaffeeCoApp.Models
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "First Name is required"), MaxLength(100)]
        public string FirstName { get; set; } = "";
        [Required(ErrorMessage = "Last Name is required"), MaxLength(100)]
        public string LastName { get; set; } = "";
        [Required(ErrorMessage = "Email is required"), EmailAddress, MaxLength(100)]
        public string Email { get; set; } = "";
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string? PhoneNumber { get; set; }
        [MaxLength(200)]
        public string? Address { get; set; } = "";
        [Required,
            RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{10,}$", ErrorMessage = "The Password must be at least 10 characters long, have capital and non-capital letters, and contain non-alphanumeric characters."),
            MaxLength(20, ErrorMessage ="The password should not exceed 20 characters.")]
        public string Password { get; set; } = "";
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = "";

    }
}
