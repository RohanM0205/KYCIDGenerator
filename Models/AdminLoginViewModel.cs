using System.ComponentModel.DataAnnotations;

namespace KYCIDGenerator.Models
{
    public class AdminLoginViewModel
    {
        [Required(ErrorMessage = "Admin Name is required")]
        [Display(Name = "Admin Name")]
        public string AdminName { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Mobile number is required")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Enter a valid 10-digit mobile number")]
        public string Mobile { get; set; }
    }
}
