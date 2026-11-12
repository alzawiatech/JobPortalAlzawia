
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using DubaiJobs.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace DubaiJobs.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100,
                ErrorMessage = "The {0} must be at least {2} characters long.",
                MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password",
                ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;

            ExternalLogins =
                (await _signInManager.GetExternalAuthenticationSchemesAsync())
                .ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;

            ExternalLogins =
                (await _signInManager.GetExternalAuthenticationSchemesAsync())
                .ToList();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var email = Input.Email.Trim();

            // Check existing email
            var existingUser = await _userManager.FindByEmailAsync(email);

            if (existingUser != null)
            {
                TempData["RegisterError"] =
                    "This email address is already registered. Please log in.";

                return RedirectToPage("./Register", new { returnUrl });
            }

            // Create Identity user
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation(
                    "A new DubaiJobs account was created successfully.");

                TempData["RegisterSuccess"] =
                    "Registration successful! Your DubaiJobs account has been created. Please log in.";

                return RedirectToPage("./Register", new { returnUrl });
            }

            foreach (var error in result.Errors)
            {
                if (error.Code == "DuplicateEmail" ||
                    error.Code == "DuplicateUserName")
                {
                    TempData["RegisterError"] =
                        "This email address is already registered. Please log in.";
                    return RedirectToPage("./Register", new { returnUrl });
                }
                else
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }
            }

            return Page();
        }
    }
}
