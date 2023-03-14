using System.ComponentModel.DataAnnotations;

namespace CSBugTracker.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? Title { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? Message { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Created { get; set; }

        public bool HasBeenViewed { get; set; }

        // Foreign Keys
        public int ProjectId { get; set; }
        public int? TicketId { get; set; }

        public int NotificationTypeId { get; set; }

        [Required]
        public string? SenderId { get; set; }

        [Required]
        public string? RecipientId { get; set; }


        // Navigation Properties

        public virtual NotificationType? NotificationType { get; set; }

        public virtual Ticket? Ticket { get; set; }

        public virtual Project? Project { get; set; }

        public virtual BTUser? Sender { get; set; }

        public virtual BTUser? Recipient { get; set; }


    }
}
