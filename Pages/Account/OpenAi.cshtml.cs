using Formify.Data;
using Formify.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Formify.Pages.Account
   
{
    public class OpenAiModel : PageModel
    {
        private readonly FormifyDbContext _db;

        public OpenAiModel(FormifyDbContext db)
        {
            _db = db;
        }

        [BindProperty]
        public string ApiKeyInput { get; set; }

        public string CurrentApiKeyMasked { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
                return RedirectToPage("/Login");

            var user = await _db.AppUsers.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return RedirectToPage("/Login");

            var key = await _db.UserApiKeys.FirstOrDefaultAsync(k => k.AppUserId == user.Id && k.KeyName == "OpenAI");
            if (key != null)
                CurrentApiKeyMasked = key.ApiKey.Length >= 8
                    ? new string('*', key.ApiKey.Length - 4) + key.ApiKey[^4..]
                    : "[zapisany klucz]";

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email)) return RedirectToPage("/Login");

            var user = await _db.AppUsers.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return RedirectToPage("/Login");

            var existing = await _db.UserApiKeys.FirstOrDefaultAsync(k => k.AppUserId == user.Id && k.KeyName == "OpenAI");
            if (existing != null)
            {
                existing.ApiKey = ApiKeyInput;
            }
            else
            {
                _db.UserApiKeys.Add(new UserApiKey
                {
                    AppUserId = user.Id,
                    KeyName = "OpenAI",
                    ApiKey = ApiKeyInput
                });
            }

            await _db.SaveChangesAsync();
            return RedirectToPage();
        }
    }
}
