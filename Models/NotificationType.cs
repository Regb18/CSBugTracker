using System.ComponentModel.DataAnnotations;

namespace CSBugTracker.Models
{
    public class NotificationType
    {
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }
    }
}
