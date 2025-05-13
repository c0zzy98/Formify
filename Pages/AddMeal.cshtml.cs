using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Formify.Models;
using Formify.Data;
using System.ComponentModel.DataAnnotations;

namespace Formify.Pages
{
    public class AddMealModel : PageModel
    {
        private readonly FormifyDbContext _db;

        public AddMealModel(FormifyDbContext db)
        {
            _db = db;
        }

        [BindProperty]
        [Required]
        public string Name { get; set; }

        [BindProperty]
        [Range(0, 5000)]
        public float Calories { get; set; }

        [BindProperty]
        [Range(0, 500)]
        public float Protein { get; set; }

        [BindProperty]
        [Range(0, 500)]
        public float Carbs { get; set; }

        [BindProperty]
        [Range(0, 500)]
        public float Fat { get; set; }

        [BindProperty]
        [Required]
        public string MealTime { get; set; } // Œniadanie, Obiad, itd.

        public string ErrorMessage { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime SelectedDate { get; set; } = DateTime.UtcNow.Date;


        public async Task<IActionResult> OnPostAsync()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
                return RedirectToPage("/Login");

            var user = _db.AppUsers.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                HttpContext.Session.Clear();
                return RedirectToPage("/Login");
            }

            if (!ModelState.IsValid)
                return Page();

            var meal = new Meal
            {
                Name = Name,
                Calories = Calories,
                Protein = Protein,
                Carbs = Carbs,
                Fat = Fat,
                MealTime = MealTime,
                Date = DateTime.SpecifyKind(SelectedDate.Date, DateTimeKind.Utc),
                AppUserId = user.Id
            };

            _db.Meals.Add(meal);
            await _db.SaveChangesAsync();

            return RedirectToPage("/Dashboard", new { SelectedDate = SelectedDate.ToString("yyyy-MM-dd") });
        }
    }
}
