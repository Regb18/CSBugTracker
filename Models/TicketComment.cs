using System.ComponentModel.DataAnnotations;

namespace CSBugTracker.Models
{
    public class TicketComment
    {
        public int Id { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? Comment { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Created { get; set; }

        // Foreign Keys

        public int TicketId { get; set; }

        [Required]
        public string? UserId { get; set; }


        // Navigation Properties

        public Ticket? Ticket { get; set; }

        public BTUser? User { get; set; }
    }
}
