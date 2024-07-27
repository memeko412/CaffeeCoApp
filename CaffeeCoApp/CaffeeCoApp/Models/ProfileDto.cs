using System.ComponentModel.DataAnnotations;

namespace CaffeeCoApp.Models
{
    public class ProfileDto
    {
        [Required(ErrorMessage = "First Name is required"), MaxLength(100), RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "First Name can only contain letters")]
        public string FirstName { get; set; } = "";
        [Required(ErrorMessage = "Last Name is required"), MaxLength(100), RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Last Name can only contain letters")]
        public string LastName { get; set; } = "";
        [Required(ErrorMessage = "Email is required"), EmailAddress, MaxLength(100)]
        public string Email { get; set; } = "";
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string? PhoneNumber { get; set; }
        [MaxLength(200), RegularExpression(@"^[a-zA-Z0-9\s\.,#-]+$", ErrorMessage = "Address can only contain alphabets, numbers, and valid symbols")]
        public string? Address { get; set; } = "";

    }
}
