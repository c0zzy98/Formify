using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Formify.Data;
using Formify.Models;

namespace Formify.Pages
{
    public class LoginModel : PageModel
    {
        private readonly FormifyDbContext _db;

        public LoginModel(FormifyDbContext db)
        {
            _db = db;
        }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _db.AppUsers
                .FirstOrDefaultAsync(u => u.Email == Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(Password, user.PasswordHash))
            {
                ErrorMessage = "Nieprawidłowy email lub hasło.";
                return Page();
            }

            // Sesja – zapamiętaj użytkownika
            HttpContext.Session.SetString("UserEmail", user.Email);

            return RedirectToPage("/Dashboard"); // dashboard stworzymy później
        }
        public IActionResult OnGet()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (!string.IsNullOrEmpty(email))
            {
                return RedirectToPage("/Dashboard");
            }

            return Page();
        }
    }
}
