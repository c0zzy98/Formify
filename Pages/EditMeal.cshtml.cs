using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Formify.Data;
using Formify.Models;

namespace Formify.Pages
{
    public class EditMealModel : PageModel
    {
        private readonly FormifyDbContext _db;

        public EditMealModel(FormifyDbContext db)
        {
            _db = db;
        }

        [BindProperty]
        public Meal Meal { get; set; } = new Meal();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email)) return RedirectToPage("/Login");

            var user = await _db.AppUsers.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return RedirectToPage("/Login");

            Meal = await _db.Meals.FirstOrDefaultAsync(m => m.Id == id && m.AppUserId == user.Id);
            if (Meal == null) return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ ModelState invalid:");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine("  - " + error.ErrorMessage);
                }
                return Page();
            }
                var email = HttpContext.Session.GetString("UserEmail");
            var user = await _db.AppUsers.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return RedirectToPage("/Login");

            var existing = await _db.Meals.FirstOrDefaultAsync(m => m.Id == Meal.Id && m.AppUserId == user.Id);
            if (existing == null) return NotFound();

            existing.Name = Meal.Name;
            existing.Calories = Meal.Calories;
            existing.Protein = Meal.Protein;
            existing.Carbs = Meal.Carbs;
            existing.Fat = Meal.Fat;
            // MealTime oraz Date nie są edytowane

            await _db.SaveChangesAsync();
            return RedirectToPage("/Dashboard", new { SelectedDate = existing.Date.ToString("yyyy-MM-dd") });
        }
    }
}
