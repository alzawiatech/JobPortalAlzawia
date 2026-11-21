
using DubaiJobs.Models;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace DubaiJobs.ViewModels
{
    public class JobApplicationViewModel
    {
        public int JobId { get; set; }

        public PostJobs? Job { get; set; }

        [Required(ErrorMessage = "Please enter your first name.")]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your last name.")]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your email address.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Please enter your cover letter.")]
        [StringLength(2000,
            ErrorMessage = "Cover letter cannot exceed 2000 characters.")]
        public string CoverLetter { get; set; } = string.Empty;

        public string? Source { get; set; }

        [Required(ErrorMessage = "Please upload your CV or resume.")]
        public IFormFile? Resume { get; set; }

        // Used to display the success panel.
        public bool IsSubmitted { get; set; } = false;

        public int? SubmittedApplicationId { get; set; }
    }
}
