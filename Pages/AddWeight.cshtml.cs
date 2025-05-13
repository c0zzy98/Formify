using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Formify.Data;
using Formify.Models;
using Microsoft.EntityFrameworkCore;

namespace Formify.Pages
{
    public class AddWeightModel : PageModel
    {
        private readonly FormifyDbContext _db;

        public AddWeightModel(FormifyDbContext db)
        {
            _db = db;
        }

        [BindProperty]
        public WeightEntry NewWeight { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public DateTime SelectedDate { get; set; } = DateTime.UtcNow.Date;


        public async Task<IActionResult> OnPostSaveWeightAsync()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email)) return RedirectToPage("/Login");

            var user = await _db.AppUsers.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return RedirectToPage("/Login");

            var targetDate = DateTime.SpecifyKind(SelectedDate.Date, DateTimeKind.Utc); // ✅ Ustawiamy Utc

            var existing = await _db.WeightEntries
                .FirstOrDefaultAsync(w => w.AppUserId == user.Id && w.EntryTime.Date == targetDate.Date);

            if (existing != null)
            {
                existing.Weight = NewWeight.Weight;
                existing.EntryTime = targetDate; // ✅ Nadpisz z Kind = Utc
                existing.Timestamp = DateTime.UtcNow;
            }
            else
            {
                _db.WeightEntries.Add(new WeightEntry
                {
                    AppUserId = user.Id,
                    Weight = NewWeight.Weight,
                    EntryTime = targetDate, // ✅ Kind = Utc
                    Timestamp = DateTime.UtcNow
                });
            }

            return RedirectToPage("/Dashboard", new { SelectedDate = targetDate.ToString("yyyy-MM-dd") });
        }


    }
}
