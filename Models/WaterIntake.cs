using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Formify.Models
{
    public class WaterIntake
    {
        public int Id { get; set; }

        [Range(0, 5000)]
        public int AmountMl { get; set; } // ilość w mililitrach

        public DateTime Date { get; set; }

        public int AppUserId { get; set; }

        [ForeignKey("AppUserId")]
        public AppUser AppUser { get; set; }
    }
}
