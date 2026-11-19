
using System;
using System.Linq;
using System.Threading.Tasks;
using DubaiJobs.Data;
using DubaiJobs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DubaiJobs.Controllers
{
    [Authorize]
    [Route("PostJob")]
    public class PostJobController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PostJobController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /PostJob/MyJobs
        [HttpGet("MyJobs")]
        public async Task<IActionResult> MyJobs()
        {
            var employerId = _userManager.GetUserId(User);

            var jobs = await _context.PostJobs
                .Where(j => j.EmployerId == employerId)
                .OrderByDescending(j => j.PostedAt)
                .ToListAsync();

            return Ok(jobs);
        }

        // POST: /PostJob/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [FromBody] JobInput model)
        {
            if (model == null)
                return BadRequest(new { message = "Invalid job details." });

            if (string.IsNullOrWhiteSpace(model.JobTitle))
                return BadRequest(new { message = "Job title is required." });

            if (model.SalaryMin < 0 ||
                model.SalaryMax < model.SalaryMin)
            {
                return BadRequest(new { message = "Invalid salary range." });
            }

            var employerId = _userManager.GetUserId(User);

            var job = new PostJobs
            {
                EmployerId = employerId,
                JobTitle = model.JobTitle,
                Department = model.Department,
                EmploymentType = model.EmploymentType,
                Workplace = model.Workplace,
                Location = model.Location,
                SalaryMin = model.SalaryMin,
                SalaryMax = model.SalaryMax,
                Description = model.Description,
                CompanyName = model.CompanyName,
                CompanyLogo = model.CompanyLogo,
                CompanyWebsite = model.CompanyWebsite,
                ContactEmail = model.ContactEmail,
                Expiry = model.Expiry,
                Category = model.Category,
                Skills = model.Skills,
                Applicants = 0,
                Status = "Active",
                PostedAt = DateTime.UtcNow
            };

            _context.PostJobs.Add(job);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Job published successfully.",
                id = job.Id
            });
        }

        // PUT: /PostJob/Edit/1
        [HttpPut("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [FromBody] JobInput model)
        {
            if (model == null)
                return BadRequest(new { message = "Invalid job details." });

            if (string.IsNullOrWhiteSpace(model.JobTitle))
                return BadRequest(new { message = "Job title is required." });

            if (model.SalaryMin < 0 ||
                model.SalaryMax < model.SalaryMin)
            {
                return BadRequest(new { message = "Invalid salary range." });
            }

            var employerId = _userManager.GetUserId(User);

            var job = await _context.PostJobs
                .FirstOrDefaultAsync(j =>
                    j.Id == id && j.EmployerId == employerId);

            if (job == null)
                return NotFound(new { message = "Job not found." });

            job.JobTitle = model.JobTitle;
            job.Department = model.Department;
            job.EmploymentType = model.EmploymentType;
            job.Workplace = model.Workplace;
            job.Location = model.Location;
            job.SalaryMin = model.SalaryMin;
            job.SalaryMax = model.SalaryMax;
            job.Description = model.Description;
            job.CompanyName = model.CompanyName;
            job.CompanyLogo = model.CompanyLogo;
            job.CompanyWebsite = model.CompanyWebsite;
            job.ContactEmail = model.ContactEmail;
            job.Expiry = model.Expiry;
            job.Category = model.Category;
            job.Skills = model.Skills;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Job updated successfully." });
        }

        // DELETE: /PostJob/Delete/1
        [HttpDelete("Delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var employerId = _userManager.GetUserId(User);

            var job = await _context.PostJobs
                .FirstOrDefaultAsync(j =>
                    j.Id == id && j.EmployerId == employerId);

            if (job == null)
                return NotFound(new { message = "Job not found." });

            _context.PostJobs.Remove(job);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Job deleted successfully." });
        }

        // POST: /PostJob/ChangeStatus/1
        [HttpPost("ChangeStatus/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(
            int id,
            [FromBody] StatusInput model)
        {
            if (model == null ||
                (model.Status != "Active" &&
                 model.Status != "Closed"))
            {
                return BadRequest(new { message = "Invalid job status." });
            }

            var employerId = _userManager.GetUserId(User);

            var job = await _context.PostJobs
                .FirstOrDefaultAsync(j =>
                    j.Id == id && j.EmployerId == employerId);

            if (job == null)
                return NotFound(new { message = "Job not found." });

            job.Status = model.Status;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Job status updated successfully."
            });
        }
    }

    // Data received from the dashboard
    public class JobInput
    {
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
    }

    public class StatusInput
    {
        public string Status { get; set; }
    }


}
