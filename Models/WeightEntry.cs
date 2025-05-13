using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Formify.Models
{
    public class WeightEntry
    {
        public int Id { get; set; }

        [Required]
        [Range(0, 300)]
        public float Weight { get; set; }

        [Required]
        public DateTime EntryTime { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public int AppUserId { get; set; }

        [ForeignKey("AppUserId")]
        public AppUser AppUser { get; set; }
    }
}
