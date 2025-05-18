using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Formify.Data;
using Formify.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Formify.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly FormifyDbContext _db;

        public RegisterModel(FormifyDbContext db)
        {
            _db = db;
        }
        [Required]
        [BindProperty]
        public string FirstName { get; set; }

        [Required]
        [BindProperty]
        public string LastName { get; set; }

        [Required(ErrorMessage = "P�e� jest wymagana")] 
        [BindProperty]
        public string Gender { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Email jest wymagany")]
        [EmailAddress(ErrorMessage = "Nieprawid�owy adres email")]
        public string Email { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Has�o jest wymagane")]
        [MinLength(6, ErrorMessage = "Has�o musi mie� co najmniej 6 znak�w")]
        public string Password { get; set; }

        [BindProperty]
        [Compare("Password", ErrorMessage = "Has�a musz� by� takie same")]
        public string ConfirmPassword { get; set; }

        [BindProperty]
        [Range(30, 300, ErrorMessage = "Wzrost musi by� mi�dzy 30 a 300 cm")]
        public int Height { get; set; }

        [BindProperty]
        [Range(10, 400, ErrorMessage = "Waga musi by� mi�dzy 10 a 400 kg")]
        public float Weight { get; set; }

        [BindProperty]
        [Range(10, 120, ErrorMessage = "Wiek musi by� mi�dzy 10 a 120 lat")]
        public int Age { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }

        [BindProperty] public Goal Goal { get; set; }
        [BindProperty] public ActivityLevel ActivityLevel { get; set; }
        [BindProperty] public WorkStyle WorkStyle { get; set; }

        public IEnumerable<SelectListItem> GoalOptions { get; set; }
        public IEnumerable<SelectListItem> ActivityOptions { get; set; }
        public IEnumerable<SelectListItem> WorkStyleOptions { get; set; }

        public string ErrorMessage { get; set; }

        public void OnGet()
        {
            LoadSelectLists();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                LoadSelectLists();
                return Page();
            }

            if (_db.AppUsers.Any(u => u.Email == Email))
            {
                ErrorMessage = "U�ytkownik o tym adresie email ju� istnieje.";
                LoadSelectLists();
                return Page();
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(Password);

            var user = new AppUser
            {
                FirstName = FirstName,
                LastName = LastName,
                Gender = Gender,
                Email = Email,
                PhoneNumber = PhoneNumber,
                PasswordHash = hashedPassword,
                Age = Age,
                Height = Height,
                Weight = Weight,
                Goal = Goal,
                ActivityLevel = ActivityLevel,
                WorkStyle = WorkStyle
            };

            _db.AppUsers.Add(user);
            await _db.SaveChangesAsync();

            return RedirectToPage("/Login");
        }

        private void LoadSelectLists()
        {
            GoalOptions = Enum.GetValues(typeof(Goal))
                .Cast<Goal>()
                .Select(g => new SelectListItem { Value = g.ToString(), Text = GetGoalText(g) });

            ActivityOptions = Enum.GetValues(typeof(ActivityLevel))
                .Cast<ActivityLevel>()
                .Select(a => new SelectListItem { Value = a.ToString(), Text = GetActivityText(a) });

            WorkStyleOptions = Enum.GetValues(typeof(WorkStyle))
                .Cast<WorkStyle>()
                .Select(w => new SelectListItem { Value = w.ToString(), Text = GetWorkStyleText(w) });
        }

        private string GetGoalText(Goal goal) => goal switch
        {
            Goal.LoseWeight => "Schudn��",
            Goal.MaintainWeight => "Utrzyma� wag�",
            Goal.BuildMuscle => "Zbudowa� mas� mi�niow�",
            _ => goal.ToString()
        };

        private string GetActivityText(ActivityLevel level) => level switch
        {
            ActivityLevel.VeryLow => "Bardzo niska � brak trening�w, ma�o ruchu w ci�gu dnia)",
            ActivityLevel.Low => "Niska � lekkie spacery lub 1-2 treningi tygodniowo",
            ActivityLevel.Medium => "�rednia � 3-4 treningi w tygodniu lub aktywny tryb �ycia",
            ActivityLevel.High => "Wysoka � 5-6 intensywnych trening�w, du�o ruchu",
            ActivityLevel.VeryHigh => "Bardzo wysoka � codzienne treningi lub praca fizyczna + trening",
            _ => level.ToString()
        };

        private string GetWorkStyleText(WorkStyle style) => style switch
        {
            WorkStyle.Sedentary => "Siedz�cy � praca przy komputerze, biuro, kierowca",
            WorkStyle.Mobile => "Ruchomy � kelner, kurier, nauczyciel, fryzjer (g��wnie chodzenie/stanie)",
            WorkStyle.Physical => "Fizyczny � magazynier, budowlaniec, mechanik (d�wiganie, intensywna praca)",
            _ => style.ToString()
        };
    }
}
