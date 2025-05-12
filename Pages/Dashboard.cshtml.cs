using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Formify.Data;
using Formify.Models;

namespace Formify.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly FormifyDbContext _db;

        public DashboardModel(FormifyDbContext db)
        {
            _db = db;
        }

        public AppUser LoggedUser { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime SelectedDate { get; set; } = DateTime.UtcNow.Date;

        public List<Meal> Meals { get; set; } = new();

        public float DailyCaloriesTarget { get; set; }
        public float DailyProteinTarget { get; set; }
        public float DailyFatTarget { get; set; }
        public float DailyCarbsTarget { get; set; }

        public float CaloriesEaten { get; set; }
        public float ProteinEaten { get; set; }
        public float FatEaten { get; set; }
        public float CarbsEaten { get; set; }

        public int CaloriesProgress => GetProgress(CaloriesEaten, DailyCaloriesTarget);
        public int ProteinProgress => GetProgress(ProteinEaten, DailyProteinTarget);
        public int FatProgress => GetProgress(FatEaten, DailyFatTarget);
        public int CarbsProgress => GetProgress(CarbsEaten, DailyCarbsTarget);

        public string GoalDisplay { get; set; }
        public string ActivityDisplay { get; set; }
        public string WorkStyleDisplay { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (SelectedDate.Kind == DateTimeKind.Unspecified)
                SelectedDate = DateTime.SpecifyKind(SelectedDate, DateTimeKind.Utc);

            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
                return RedirectToPage("/Login");

            LoggedUser = await _db.AppUsers.FirstOrDefaultAsync(u => u.Email == email);
            if (LoggedUser == null)
            {
                HttpContext.Session.Clear();
                return RedirectToPage("/Login");
            }

            GoalDisplay = GetGoalText(LoggedUser.Goal);
            ActivityDisplay = GetActivityText(LoggedUser.ActivityLevel);
            WorkStyleDisplay = GetWorkStyleText(LoggedUser.WorkStyle);

            CalculateUserNeeds(LoggedUser);

            Meals = await _db.Meals
                .Where(m => m.AppUserId == LoggedUser.Id && m.Date.Date == SelectedDate.Date)
                .ToListAsync();

            CaloriesEaten = Meals.Sum(m => m.Calories);
            ProteinEaten = Meals.Sum(m => m.Protein);
            FatEaten = Meals.Sum(m => m.Fat);
            CarbsEaten = Meals.Sum(m => m.Carbs);

            return Page();
        }

        private void CalculateUserNeeds(AppUser user)
        {
            float bmr = 10f * user.Weight + 6.25f * user.Height - 5f * user.Age + 5f;

            float activityMultiplier = user.ActivityLevel switch
            {
                ActivityLevel.VeryLow => 1.2f,
                ActivityLevel.Low => 1.375f,
                ActivityLevel.Medium => 1.55f,
                ActivityLevel.High => 1.725f,
                ActivityLevel.VeryHigh => 1.9f,
                _ => 1.55f
            };

            float tee = bmr * activityMultiplier;

            float goalModifier = user.Goal switch
            {
                Goal.LoseWeight => 0.85f,
                Goal.MaintainWeight => 1.0f,
                Goal.BuildMuscle => 1.15f,
                _ => 1.0f
            };

            DailyCaloriesTarget = tee * goalModifier;

            DailyProteinTarget = user.Weight * 2f;
            DailyFatTarget = user.Weight * 1f;

            float proteinKcal = DailyProteinTarget * 4f;
            float fatKcal = DailyFatTarget * 9f;
            float carbsKcal = DailyCaloriesTarget - proteinKcal - fatKcal;
            DailyCarbsTarget = carbsKcal / 4f;
        }

        private int GetProgress(float consumed, float target)
        {
            if (target <= 0) return 0;
            return (int)Math.Round((consumed / target) * 100);
        }

        public string GetGoalText(Goal goal) => goal switch
        {
            Goal.LoseWeight => "Schudn¹æ",
            Goal.MaintainWeight => "Utrzymaæ wagê",
            Goal.BuildMuscle => "Zbudowaæ masê miêœniow¹",
            _ => goal.ToString()
        };

        public string GetActivityText(ActivityLevel level) => level switch
        {
            ActivityLevel.VeryLow => "Bardzo niska (np. praca siedz¹ca, brak ruchu)",
            ActivityLevel.Low => "Niska (spacery, obowi¹zki domowe)",
            ActivityLevel.Medium => "Œrednia (chodzenie, stanie, czêsty ruch)",
            ActivityLevel.High => "Wysoka (du¿o ruchu)",
            ActivityLevel.VeryHigh => "Bardzo wysoka (ciê¿ka praca fizyczna, sportowiec)",
            _ => level.ToString()
        };

        public string GetWorkStyleText(WorkStyle style) => style switch
        {
            WorkStyle.Sedentary => "Siedz¹cy (np. biurowy)",
            WorkStyle.Mobile => "Ruchomy (np. kelner, kurier)",
            WorkStyle.Physical => "Fizyczny (np. magazyn, budowa)",
            _ => style.ToString()
        };
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email)) return RedirectToPage("/Login");

            var user = await _db.AppUsers.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return RedirectToPage("/Login");

            var meal = await _db.Meals.FirstOrDefaultAsync(m => m.Id == id && m.AppUserId == user.Id);
            if (meal != null)
            {
                _db.Meals.Remove(meal);
                await _db.SaveChangesAsync();
            }

            return RedirectToPage(new { SelectedDate = SelectedDate.ToString("yyyy-MM-dd") });
        }

    }
}
