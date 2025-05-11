using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Formify.Data;
using Formify.Models;
using Microsoft.EntityFrameworkCore;

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

        public async Task<IActionResult> OnGetAsync()
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(email))
                return RedirectToPage("/Login");

            LoggedUser = await _db.AppUsers.FirstOrDefaultAsync(u => u.Email == email);

            GoalDisplay = GetGoalText(LoggedUser.Goal);
            ActivityDisplay = GetActivityText(LoggedUser.ActivityLevel);
            WorkStyleDisplay = GetWorkStyleText(LoggedUser.WorkStyle);

            if (LoggedUser == null)
            {
                HttpContext.Session.Clear();
                return RedirectToPage("/Login");
            }

            return Page();
        }
        public string GetGoalText(Goal goal) => goal switch
        {
            Goal.LoseWeight => "Schudn��",
            Goal.MaintainWeight => "Utrzyma� wag�",
            Goal.BuildMuscle => "Zbudowa� mas� mi�niow�",
            _ => goal.ToString()
        };

        public string GetActivityText(ActivityLevel level) => level switch
        {
            ActivityLevel.VeryLow => "Bardzo niska (np. praca siedz�ca, brak ruchu)",
            ActivityLevel.Low => "Niska (spacery, obowi�zki domowe)",
            ActivityLevel.Medium => "�rednia (chodzenie, stanie, cz�sty ruch)",
            ActivityLevel.High => "Wysoka (du�o ruchu)",
            ActivityLevel.VeryHigh => "Bardzo wysoka (ci�ka praca fizyczna, sportowiec)",
            _ => level.ToString()
        };

        public string GetWorkStyleText(WorkStyle style) => style switch
        {
            WorkStyle.Sedentary => "Siedz�cy (np. biurowy)",
            WorkStyle.Mobile => "Ruchomy (np. kelner, kurier)",
            WorkStyle.Physical => "Fizyczny (np. magazyn, budowa)",
            _ => style.ToString()
        };
        public string GoalDisplay { get; set; }
        public string ActivityDisplay { get; set; }
        public string WorkStyleDisplay { get; set; }

    }
}
