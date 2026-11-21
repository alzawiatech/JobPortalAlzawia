
using System;
using System.ComponentModel.DataAnnotations;

namespace DubaiJobs.Models
{
    public class JobApplication
    {
        public int Id { get; set; }

        [Required]
        public string ApplicantId { get; set; }

        public int JobId { get; set; }

        [Required, StringLength(100)]
        public string FirstName { get; set; }

        [Required, StringLength(100)]
        public string LastName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        public string Phone { get; set; }

        [Required]
        public string ResumePath { get; set; }

        [Required]
        public string CoverLetter { get; set; }

        public string Source { get; set; }

        public string Status { get; set; } = "Under Review";

        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

        public PostJobs Job { get; set; }
        public ApplicationUser Applicant { get; set; }
    }
}
