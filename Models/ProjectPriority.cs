using System.ComponentModel.DataAnnotations;

namespace CSBugTracker.Models
{
    public class ProjectPriority
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? Name { get; set; }
    }
}
