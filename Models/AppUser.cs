using Formify.Models;

namespace Formify.Models
{
    public class AppUser
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }

        // Nowe dane
        public int Age { get; set; }
        public int Height { get; set; } // cm
        public float Weight { get; set; } // kg

        public Goal Goal { get; set; }
        public ActivityLevel ActivityLevel { get; set; }
        public WorkStyle WorkStyle { get; set; }
        public List<Meal> Meals { get; set; }

    }
}

public enum Goal
{
    LoseWeight,
    MaintainWeight,
    BuildMuscle
}

public enum ActivityLevel
{
    VeryLow,
    Low,
    Medium,
    High,
    VeryHigh
}

public enum WorkStyle
{
    Sedentary,
    Mobile,
    Physical
}

