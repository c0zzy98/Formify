using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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

        public string? MealTime { get; set; } // np. Śniadanie, Obiad

        public DateTime Date { get; set; }

        public int AppUserId { get; set; }

        [ForeignKey("AppUserId")]
        [BindNever] // ⬅️ Dodane, aby nie bindować AppUser z formularza
        public AppUser? AppUser { get; set; }
    }
}
