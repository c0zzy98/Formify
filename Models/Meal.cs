using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Formify.Models;

namespace Formify.Models
{
    public class Meal
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Range(0, 5000)]
        public float Calories { get; set; }

        [Range(0, 500)]
        public float Protein { get; set; }

        [Range(0, 500)]
        public float Carbs { get; set; }

        [Range(0, 500)]
        public float Fat { get; set; }

        [Required]
        public string MealTime { get; set; } // np. Śniadanie, Obiad

        public DateTime Date { get; set; }

        // Powiązanie z użytkownikiem
        public int AppUserId { get; set; }

        [ForeignKey("AppUserId")]
        public AppUser AppUser { get; set; }
    }
}
