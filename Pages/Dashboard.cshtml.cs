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

            LoggedUser = await _db.AppUsers
                .FirstOrDefaultAsync(u => u.Email == email);

            if (LoggedUser == null)
            {
                HttpContext.Session.Clear();
                return RedirectToPage("/Login");
            }

            // Poka¿ dane formatowane
            GoalDisplay = GetGoalText(LoggedUser.Goal);
            ActivityDisplay = GetActivityText(LoggedUser.ActivityLevel);
            WorkStyleDisplay = GetWorkStyleText(LoggedUser.WorkStyle);

            // Pobierz posi³ki danego u¿ytkownika z wybranej daty
            Meals = await _db.Meals
                .Where(m => m.AppUserId == LoggedUser.Id && m.Date.Date == SelectedDate.Date)
                .OrderBy(m => m.MealTime)
                .ToListAsync();

            return Page();
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
    }
}
