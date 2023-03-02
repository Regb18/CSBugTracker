using System.ComponentModel.DataAnnotations;

namespace CSBugTracker.Models
{
    public class TicketPriority
    {
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }
    }
}
