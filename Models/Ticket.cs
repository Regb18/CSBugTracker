using System.ComponentModel.DataAnnotations;

namespace CSBugTracker.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? Title { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? Description { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Created { get; set; }


        [DataType(DataType.Date)]
        public DateTime? Updated { get; set; }

        public bool Archived { get; set; }
        public bool ArchivedByProject { get; set; }

        // Foreign Keys

        public int ProjectId { get; set; }
        public int TicketTypeId { get; set; }
        public int TicketStatusId { get; set; }
        public int TicketPriorityId { get; set; }

        public string? DeveloperUserId { get; set; }

        [Required]
        public string? SubmitterUserId { get; set; }


        // Navigation Properties

        public Project? Project { get; set; }

        public TicketPriority? TicketPriority { get; set; }
        public TicketType? TicketType { get; set; }
        public TicketStatus? TicketStatus { get; set; }
        public BTUser? DeveloperUser { get; set; }
        public BTUser? SubmitterUser { get; set; }

        public virtual ICollection<TicketComment> TicketComments { get; set; } = new HashSet<TicketComment>();
        public virtual ICollection<TicketAttachment> TicketAttachments { get; set; } = new HashSet<TicketAttachment>();
        public virtual ICollection<TicketHistory> TicketHistory { get; set; } = new HashSet<TicketHistory>();

    }
}
