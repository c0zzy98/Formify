using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Formify.Models;

namespace Formify.Models
{
    public class WaterIntake
    {
        public int Id { get; set; }

        [ForeignKey("AppUserId")]
        public int AppUserId { get; set; }
        public AppUser AppUser { get; set; }

        [Range(0, 5000)]
        public int AmountMl { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow.Date;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // nowa kolumna
    }
}
