using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using TheGreenBowl.Services;

namespace TheGreenBowl.Pages
{
    public class ContactModel : PageModel
    {
        private readonly ILogger<ContactModel> _logger;
        private readonly IEmailService _emailService;

        public ContactModel(ILogger<ContactModel> logger, IEmailService emailService)
        {
            _logger = logger;
            _emailService = emailService;
        }

        [BindProperty]
        public ContactFormData FormData { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Use the email service to "send" the email
                await _emailService.SendEmailAsync(
                    to: "restaurant@thegreenbowl.com",
                    from: FormData.Email,
                    subject: FormData.Subject,
                    body: $"Name: {FormData.Name}\nEmail: {FormData.Email}\n\n{FormData.Message}"
                );

                // Set success message
                TempData["SuccessMessage"] = "Thank you for your message! We'll get back to you soon.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending contact email");
                ModelState.AddModelError(string.Empty, "There was a problem sending your message. Please try again later.");
                return Page();
            }

            // Redirect to the same page to avoid form resubmission
            return RedirectToPage();
        }
    }

    public class ContactFormData
    {
        [Required(ErrorMessage = "Please enter your name")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter your email address")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter a subject")]
        [StringLength(200, ErrorMessage = "Subject cannot be longer than 200 characters")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Please enter your message")]
        public string Message { get; set; }
    }
}
