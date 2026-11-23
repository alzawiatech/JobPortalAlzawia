
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DubaiJobs.Data;
using DubaiJobs.Models;
using DubaiJobs.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DubaiJobs.Controllers
{
    [Authorize]
    public class ApplyJobController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ApplyJobController> _logger;

        private const long MaxResumeSize = 5 * 1024 * 1024;

        private static readonly string[] AllowedExtensions =
        {
            ".pdf",
            ".doc",
            ".docx"
        };

        private const string ApplyView =
            "~/Views/Home/ApplyJobs.cshtml";

        private const string ApplicationsView =
            "~/Views/Home/MyApplications.cshtml";

        public ApplyJobController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment,
            ILogger<ApplyJobController> logger)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
            _logger = logger;
        }

        // GET: /ApplyJob/Apply?jobId=1
        [HttpGet]
        public async Task<IActionResult> Apply(int jobId)
        {
            var job = await _context.PostJobs
                .AsNoTracking()
                .FirstOrDefaultAsync(j =>
                    j.Id == jobId &&
                    j.Status == "Active");

            if (job == null)
            {
                return NotFound(
                    "Job not found or no longer available.");
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var model = new JobApplicationViewModel
            {
                JobId = job.Id,
                Job = job,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.PhoneNumber,
                IsSubmitted = false
            };

            return View(ApplyView, model);
        }

        // POST: /ApplyJob/Apply
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> Apply(
            JobApplicationViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            // Find the selected job.
            var job = await _context.PostJobs
                .FirstOrDefaultAsync(j =>
                    j.Id == model.JobId &&
                    j.Status == "Active");

            if (job == null)
            {
                return NotFound(
                    "This job is no longer available.");
            }

            // Reload job details for the view.
            model.Job = job;
            model.IsSubmitted = false;

            // Check whether the user has already applied.
            var alreadyApplied = await _context.JobApplications
                .AnyAsync(a =>
                    a.ApplicantId == user.Id &&
                    a.JobId == job.Id);

            if (alreadyApplied)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "You have already applied for this job.");

                return View(ApplyView, model);
            }

            // Validate the uploaded resume.
            if (model.Resume == null ||
                model.Resume.Length == 0)
            {
                ModelState.AddModelError(
                    nameof(model.Resume),
                    "Please select your CV or resume.");
            }
            else
            {
                var extension = Path.GetExtension(
                    model.Resume.FileName)?.ToLowerInvariant();

                if (string.IsNullOrWhiteSpace(extension) ||
                    !AllowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        nameof(model.Resume),
                        "Only PDF, DOC and DOCX files are allowed.");
                }

                if (model.Resume.Length > MaxResumeSize)
                {
                    ModelState.AddModelError(
                        nameof(model.Resume),
                        "Your CV must not exceed 5 MB.");
                }
            }

            // Return the form if validation fails.
            if (!ModelState.IsValid)
            {
                return View(ApplyView, model);
            }

            // Save resumes outside wwwroot.
            var uploadFolder = Path.Combine(
                _environment.ContentRootPath,
                "PrivateUploads",
                "Resumes");

            string savedFileName = null;
            string filePath = null;

            try
            {
                Directory.CreateDirectory(uploadFolder);

                var extension = Path.GetExtension(
                    model.Resume.FileName).ToLowerInvariant();

                savedFileName =
                    Guid.NewGuid().ToString("N") + extension;

                filePath = Path.Combine(
                    uploadFolder,
                    savedFileName);

                // Save uploaded resume.
                await using (var stream = new FileStream(
                    filePath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    81920,
                    useAsync: true))
                {
                    await model.Resume.CopyToAsync(stream);
                }

                // Create the application record.
                var application = new JobApplication
                {
                    ApplicantId = user.Id,
                    JobId = job.Id,

                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Phone = model.Phone,

                    ResumePath = savedFileName,
                    CoverLetter = model.CoverLetter,
                    Source = model.Source,

                    Status = "Under Review",
                    AppliedAt = DateTime.UtcNow,

                    Job = job
                };

                _context.JobApplications.Add(application);

                // Increase applicant count.
                job.Applicants++;

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Application submitted by user {UserId} for job {JobId}.",
                    user.Id,
                    job.Id);

                // Show success panel on the same page.
                model.IsSubmitted = true;
                model.SubmittedApplicationId = application.Id;
                model.Job = job;

                // Clear old validation errors.
                ModelState.Clear();

                return View(ApplyView, model);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(
                    ex,
                    "Database error while saving application for job {JobId}.",
                    job.Id);

                DeleteUploadedFile(filePath);

                ModelState.AddModelError(
                    string.Empty,
                    "Your application could not be saved. " +
                    "Please check whether you have already applied and try again.");

                return View(ApplyView, model);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while submitting application for job {JobId}.",
                    job.Id);

                DeleteUploadedFile(filePath);

                ModelState.AddModelError(
                    string.Empty,
                    "An unexpected error occurred while submitting your application. " +
                    "Please try again.");

                return View(ApplyView, model);
            }
        }

        // Delete uploaded CV if saving fails.
        private void DeleteUploadedFile(string filePath)
        {
            if (!string.IsNullOrWhiteSpace(filePath) &&
                System.IO.File.Exists(filePath))
            {
                try
                {
                    System.IO.File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Could not delete uploaded resume.");
                }
            }
        }

        // GET: /ApplyJob/MyApplications
        [HttpGet]
        public async Task<IActionResult> MyApplications()
        {
            var userId = _userManager.GetUserId(User);

            var applications = await _context.JobApplications
                .AsNoTracking()
                .Include(a => a.Job)
                .Where(a => a.ApplicantId == userId)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();

            return View(ApplicationsView, applications);
        }


        // GET: /ApplyJob/MyApplicationsJson
        [HttpGet]
        public async Task<IActionResult> MyApplicationsJson()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var applications = await _context.JobApplications
                .AsNoTracking()
                .Where(a => a.ApplicantId == userId)
                .OrderByDescending(a => a.AppliedAt)
                .Select(a => new
                {
                    applicationId = a.Id,
                    jobId = a.JobId,

                    jobTitle = a.Job.JobTitle,
                    companyName = a.Job.CompanyName,
                    location = a.Job.Location,

                    employmentType = a.Job.EmploymentType,
                    workplace = a.Job.Workplace,

                    appliedAt = a.AppliedAt,
                    status = a.Status
                })
                .ToListAsync();

            return Json(applications);
        }


        // GET: /ApplyJob/ApplicantsJson?jobId=1
        [HttpGet]
        public async Task<IActionResult> ApplicantsJson(int jobId)
        {
            var employerId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(employerId))
            {
                return Unauthorized();
            }

            // Make sure this job belongs to the logged-in employer.
            var job = await _context.PostJobs
                .AsNoTracking()
                .FirstOrDefaultAsync(j =>
                    j.Id == jobId &&
                    j.EmployerId == employerId);

            if (job == null)
            {
                return NotFound(new
                {
                    message = "Job not found or you do not have permission to view its applicants."
                });
            }

            var applicants = await _context.JobApplications
                .AsNoTracking()
                .Where(a => a.JobId == jobId)
                .OrderByDescending(a => a.AppliedAt)
                .Select(a => new
                {
                    id = a.Id,
                    name = a.FirstName + " " + a.LastName,
                    email = a.Email,
                    phone = a.Phone,
                    coverLetter = a.CoverLetter,
                    source = a.Source,
                    appliedAt = a.AppliedAt,
                    status = a.Status
                })
                .ToListAsync();

            return Json(applicants);
        }

        // POST: /ApplyJob/UpdateApplicantStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateApplicantStatus(
            int applicationId,
            string status)
        {
            var employerId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(employerId))
            {
                return Unauthorized();
            }

            if (status != "Shortlisted" && status != "Rejected")
            {
                return BadRequest(new
                {
                    message = "Invalid applicant status."
                });
            }

            // Only allow an employer to update applications
            // belonging to one of their own jobs.
            var application = await _context.JobApplications
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a =>
                    a.Id == applicationId &&
                    a.Job.EmployerId == employerId);

            if (application == null)
            {
                return NotFound(new
                {
                    message = "Application not found or access denied."
                });
            }

            application.Status = status;

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                applicationId = application.Id,
                status = application.Status
            });
        }


        // GET: /ApplyJob/Resume?applicationId=1
        [HttpGet]
        public async Task<IActionResult> Resume(
            int applicationId,
            bool download = false)
        {
            var employerId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(employerId))
            {
                return Unauthorized();
            }

            var application = await _context.JobApplications
                .AsNoTracking()
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a =>
                    a.Id == applicationId &&
                    a.Job.EmployerId == employerId);

            if (application == null)
            {
                return NotFound(
                    "Application not found or access denied.");
            }

            var fileName = application.ResumePath;

            if (string.IsNullOrWhiteSpace(fileName) ||
                fileName != Path.GetFileName(fileName))
            {
                return NotFound("Resume not found.");
            }

            var extension = Path.GetExtension(fileName)
                .ToLowerInvariant();

            var contentType = extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                _ => null
            };

            if (contentType == null)
            {
                return BadRequest("Unsupported resume file type.");
            }

            var filePath = Path.Combine(
                _environment.ContentRootPath,
                "PrivateUploads",
                "Resumes",
                fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Resume file does not exist.");
            }

            if (download)
            {
                return PhysicalFile(
                    filePath,
                    contentType,
                    fileName);
            }

            Response.Headers["Content-Disposition"] =
                $"inline; filename=\"{fileName}\"";

            return PhysicalFile(filePath, contentType);
        }



    }
}
