using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TheGreenBowl.Data;
using TheGreenBowl.Models;
using TheGreenBowl.Services;
using Microsoft.Extensions.Logging;

namespace TheGreenBowl.Pages
{
    public class ContactModel : PageModel
    {
        private readonly TheGreenBowlContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<ContactModel> _logger;

        public ContactModel(TheGreenBowlContext context,
                            IEmailService emailService,
                            ILogger<ContactModel> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        // This property holds the contact form data.
        [BindProperty]
        public ContactFormData FormData { get; set; }

        // New property: This list will hold the opening times retrieved from the database.
        public List<tblOpeningTimes> OpeningTimes { get; set; } = new List<tblOpeningTimes>();

        public async Task<IActionResult> OnGetAsync()
        {
            // Load the active opening times. Adjust the filter as needed.
            OpeningTimes = await _context.tblOpeningTimes
                .Where(ot => ot.IsEnabled && (!ot.EnabledUntil.HasValue || ot.EnabledUntil > DateTime.Now))
                .OrderBy(ot => ot.DayOfWeek)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Use the email service to "send" the email.
                await _emailService.SendEmailAsync(
                    to: "restaurant@thegreenbowl.com",
                    from: FormData.Email,
                    subject: FormData.Subject,
                    body: $"Name: {FormData.Name}\nEmail: {FormData.Email}\n\n{FormData.Message}"
                );

                TempData["SuccessMessage"] = "Thank you for your message! We'll get back to you soon.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending contact email");
                ModelState.AddModelError(string.Empty, "There was a problem sending your message. Please try again later.");
                return Page();
            }

            // Redirect to avoid resubmission
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
