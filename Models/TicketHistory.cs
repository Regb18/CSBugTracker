using System.ComponentModel.DataAnnotations;

namespace CSBugTracker.Models
{
    public class TicketHistory
    {
        public int Id { get; set; }

        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? PropertyName { get; set; }


        [StringLength(1000, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? Description { get; set; }


        [StringLength(1000, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? OldValue { get; set; }


        [StringLength(1000, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? NewValue { get; set; }


        // Foreign Keys
        public int TicketId { get; set; }

        [Required]
        public string? UserId { get; set; }


        // Navigation Properties

        public Ticket? Ticket { get; set; }

        public BTUser? User { get; set; }
    }
}
