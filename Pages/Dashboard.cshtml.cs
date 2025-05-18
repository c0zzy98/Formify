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
        public List<WeightEntry> TodaysWeights { get; set; } = new();

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
        public int WaterDrunkToday { get; set; }
        public int WaterTarget => 3000;
        public int WaterProgress => (int)Math.Min(100, Math.Round((double)WaterDrunkToday / WaterTarget * 100));

        public string GoalDisplay { get; set; }
        public string ActivityDisplay { get; set; }
        public string WorkStyleDisplay { get; set; }

        [BindProperty]
        public float WeightInput { get; set; }

        public float? TodaysWeightValue { get; set; }

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

            // 🆕 Pobierz kontekst użytkownika na wybrany dzień
            var contextSnapshot = await _db.UserContextChanges
                .Where(c => c.AppUserId == LoggedUser.Id && c.EffectiveFrom <= SelectedDate)
                .OrderByDescending(c => c.EffectiveFrom)
                .FirstOrDefaultAsync();

            if (contextSnapshot != null)
            {
                LoggedUser.Goal = contextSnapshot.Goal;
                LoggedUser.ActivityLevel = contextSnapshot.ActivityLevel;
                LoggedUser.WorkStyle = contextSnapshot.WorkStyle;
            }

            WaterDrunkToday = await _db.WaterIntakes
                .Where(w => w.AppUserId == LoggedUser.Id && w.Date.Date == SelectedDate.Date)
                .SumAsync(w => w.AmountMl);

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

            TodaysWeightValue = await _db.WeightEntries
                .Where(w => w.AppUserId == LoggedUser.Id && w.EntryTime.Date == SelectedDate.Date)
                .OrderByDescending(w => w.EntryTime)
                .Select(w => (float?)w.Weight)
                .FirstOrDefaultAsync();

            return Page();
        }

        private void CalculateUserNeeds(AppUser user)

        {
            float bmr;

            if (user.Gender?.ToLower() == "kobieta")
            {
                bmr = 10f * user.Weight + 6.25f * user.Height - 5f * user.Age - 161f;
            }
            else
            {
                // Domyślnie: mężczyzna lub inna płeć – stosujemy wzór męski
                bmr = 10f * user.Weight + 6.25f * user.Height - 5f * user.Age + 5f;
            }

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
            Goal.LoseWeight => "Schudnąć",
            Goal.MaintainWeight => "Utrzymać wagę",
            Goal.BuildMuscle => "Zbudować masę mięśniową",
            _ => goal.ToString()
        };

        public string GetActivityText(ActivityLevel level) => level switch
        {
            ActivityLevel.VeryLow => "Bardzo niska (np. praca siedząca, brak ruchu)",
            ActivityLevel.Low => "Niska (spacery, obowiązki domowe)",
            ActivityLevel.Medium => "Średnia (chodzenie, stanie, częsty ruch)",
            ActivityLevel.High => "Wysoka (dużo ruchu)",
            ActivityLevel.VeryHigh => "Bardzo wysoka (ciężka praca fizyczna, sportowiec)",
            _ => level.ToString()
        };

        public string GetWorkStyleText(WorkStyle style) => style switch
        {
            WorkStyle.Sedentary => "Siedzący (np. biurowy)",
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

        public async Task<IActionResult> OnPostAddWaterAsync(int amount)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var user = await _db.AppUsers.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return RedirectToPage("/Login");

            var water = new WaterIntake
            {
                AppUserId = user.Id,
                AmountMl = amount,
                Date = DateTime.SpecifyKind(SelectedDate, DateTimeKind.Utc)
            };

            _db.WaterIntakes.Add(water);
            await _db.SaveChangesAsync();

            return RedirectToPage(new { SelectedDate = SelectedDate.ToString("yyyy-MM-dd") });
        }

        public async Task<IActionResult> OnPostUndoWaterAsync()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email)) return RedirectToPage("/Login");

            var user = await _db.AppUsers.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return RedirectToPage("/Login");

            var safeDate = DateTime.SpecifyKind(SelectedDate.Date, DateTimeKind.Utc);

            var lastEntry = await _db.WaterIntakes
                .Where(w => w.AppUserId == user.Id && w.Date.Date == safeDate)
                .OrderByDescending(w => w.CreatedAt)
                .FirstOrDefaultAsync();

            if (lastEntry != null)
            {
                _db.WaterIntakes.Remove(lastEntry);
                await _db.SaveChangesAsync();
            }

            return RedirectToPage(new { SelectedDate = SelectedDate.ToString("yyyy-MM-dd") });
        }

        public async Task<IActionResult> OnPostSaveWeightAsync()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email)) return RedirectToPage("/Login");

            var user = await _db.AppUsers.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return RedirectToPage("/Login");

            SelectedDate = DateTime.SpecifyKind(SelectedDate, DateTimeKind.Utc);
            var targetDate = SelectedDate.Date;

            var existing = await _db.WeightEntries
                .FirstOrDefaultAsync(w => w.AppUserId == user.Id && w.EntryTime.Date == targetDate);

            if (existing != null)
            {
                existing.Weight = WeightInput;
                existing.EntryTime = targetDate;
                existing.Timestamp = DateTime.UtcNow;
            }
            else
            {
                _db.WeightEntries.Add(new WeightEntry
                {
                    AppUserId = user.Id,
                    Weight = WeightInput,
                    EntryTime = targetDate,
                    Timestamp = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync();
            return RedirectToPage("/Dashboard", new { SelectedDate = targetDate.ToString("yyyy-MM-dd") });
        }
    }
}

    
