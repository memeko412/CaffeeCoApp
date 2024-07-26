using System.ComponentModel.DataAnnotations;

namespace CaffeeCoApp.Models
{
    public class ProfileDto
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

    }
}
