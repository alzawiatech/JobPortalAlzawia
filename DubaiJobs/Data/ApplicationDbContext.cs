
using DubaiJobs.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DubaiJobs.Data
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Job postings
        public DbSet<PostJobs> PostJobs { get; set; }

        // Candidate job applications
        public DbSet<JobApplication> JobApplications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Job Application -> Job relationship
            builder.Entity<JobApplication>()
                .HasOne(a => a.Job)
                .WithMany()
                .HasForeignKey(a => a.JobId)
                .OnDelete(DeleteBehavior.Restrict);

            // Job Application -> Candidate relationship
            builder.Entity<JobApplication>()
                .HasOne(a => a.Applicant)
                .WithMany()
                .HasForeignKey(a => a.ApplicantId)
                .OnDelete(DeleteBehavior.Restrict);

            // Prevent duplicate applications for the same job
            // by the same candidate
            builder.Entity<JobApplication>()
                .HasIndex(a => new
                {
                    a.ApplicantId,
                    a.JobId
                })
                .IsUnique();
        }
    }
}
