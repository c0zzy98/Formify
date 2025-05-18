using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Formify.Models
{
    public class UserApiKey
    {
        public int Id { get; set; }

        [Required]
        public int AppUserId { get; set; }

        [Required]
        public string KeyName { get; set; } // np. "OpenAI"

        [Required]
        public string ApiKey { get; set; }

        [ForeignKey("AppUserId")]
        public AppUser AppUser { get; set; }
    }
}
