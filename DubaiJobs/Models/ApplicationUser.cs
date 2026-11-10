using Microsoft.AspNetCore.Identity;
namespace DubaiJobs.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
    }
}