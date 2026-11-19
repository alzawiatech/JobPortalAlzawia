
using System;
using System.ComponentModel.DataAnnotations;

namespace DubaiJobs.Models
{
    public class PostJobs
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string EmployerId { get; set; }

        public string JobTitle { get; set; }
        public string Department { get; set; }
        public string EmploymentType { get; set; }
        public string Workplace { get; set; }
        public string Location { get; set; }

        public decimal SalaryMin { get; set; }
        public decimal SalaryMax { get; set; }

        public string Description { get; set; }
        public string CompanyName { get; set; }
        public string CompanyLogo { get; set; }
        public string CompanyWebsite { get; set; }
        public string ContactEmail { get; set; }

        public DateTime? Expiry { get; set; }
        public string Category { get; set; }
        public string Skills { get; set; }

        public int Applicants { get; set; } = 0;
        public string Status { get; set; } = "Active";

        public DateTime PostedAt { get; set; } = DateTime.UtcNow;
    }
}
