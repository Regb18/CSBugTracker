using System.ComponentModel.DataAnnotations;

namespace CSBugTracker.Models
{
    public class TicketStatus
    {
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }
    }
}
