using Formify.Data;
using Formify.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Formify.Pages.Account
{
    public class ProfileModel : PageModel
    {
        private readonly FormifyDbContext _db;

        public ProfileModel(FormifyDbContext db)
        {
            _db = db;
        }

        [BindProperty]
        public Goal UpdatedGoal { get; set; }

        [BindProperty]
        public ActivityLevel UpdatedActivityLevel { get; set; }

        [BindProperty]
        public WorkStyle UpdatedWorkStyle { get; set; }

        [BindProperty]
        public string PhoneNumber { get; set; }

        [BindProperty]
        public string CurrentPassword { get; set; }

        [BindProperty]
        public string NewPassword { get; set; }

        [BindProperty]
        public string ConfirmPassword { get; set; }

        public string PasswordChangeMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
                return RedirectToPage("/Login");

            var user = await _db.AppUsers.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return RedirectToPage("/Login");

            // Pobierz ostatni kontekst u¿ytkownika
            var lastContext = await _db.UserContextChanges
                .Where(c => c.AppUserId == user.Id)
                .OrderByDescending(c => c.EffectiveFrom)
                .FirstOrDefaultAsync();

            if (lastContext != null)
            {
                UpdatedGoal = lastContext.Goal;
                UpdatedActivityLevel = lastContext.ActivityLevel;
                UpdatedWorkStyle = lastContext.WorkStyle;
            }
            PhoneNumber = user.PhoneNumber;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
                return RedirectToPage("/Login");

            var user = await _db.AppUsers.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return RedirectToPage("/Login");

            var newContext = new UserContextChange
            {
                AppUserId = user.Id,
                Goal = UpdatedGoal,
                ActivityLevel = UpdatedActivityLevel,
                WorkStyle = UpdatedWorkStyle,
                EffectiveFrom = DateTime.UtcNow.Date
            };

            _db.UserContextChanges.Add(newContext);
            await _db.SaveChangesAsync();
            user.PhoneNumber = PhoneNumber;

            if (!string.IsNullOrEmpty(CurrentPassword) &&
                !string.IsNullOrEmpty(NewPassword) &&
                !string.IsNullOrEmpty(ConfirmPassword))
            {
                if (!BCrypt.Net.BCrypt.Verify(CurrentPassword, user.PasswordHash))
                {
                    PasswordChangeMessage = "Obecne has³o jest niepoprawne.";
                    return Page();
                }

                if (NewPassword != ConfirmPassword)
                {
                    PasswordChangeMessage = "Nowe has³a nie s¹ identyczne.";
                    return Page();
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(NewPassword);
            }
            await _db.SaveChangesAsync();

            return RedirectToPage("/Dashboard");
        }
    }
}
