
using System.ComponentModel.DataAnnotations;

namespace DubaiJobs.Models.ViewModels
{
    public class EmployerRegisterViewModel
    {
        [Required(ErrorMessage = "First name is required.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Company name is required.")]
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please confirm your password.")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Range(typeof(bool), "true", "true",
            ErrorMessage = "You must accept the Terms and Conditions.")]
        public bool AcceptTerms { get; set; }
    }
}
