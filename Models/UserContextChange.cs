using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Formify.Models
{
    public class UserContextChange
    {
        public int Id { get; set; }

        [Required]
        public int AppUserId { get; set; }

        [ForeignKey("AppUserId")]
        public AppUser AppUser { get; set; }

        [Required]
        public Goal Goal { get; set; }

        [Required]
        public ActivityLevel ActivityLevel { get; set; }

        [Required]
        public WorkStyle WorkStyle { get; set; }

        [Required]
        public DateTime EffectiveFrom { get; set; }
    }
}
