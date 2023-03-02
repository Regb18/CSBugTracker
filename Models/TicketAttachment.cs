using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CSBugTracker.Models
{
    public class TicketAttachment
    {
        public int Id { get; set; }

        [StringLength(1000, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? Description { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Created { get; set; }

        public byte[]? ImageData { get; set; }
        public string? ImageType { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        // Foreign Keys

        public int TicketId { get; set; }

        [Required]
        public string? UserId { get; set; }

        // Navigation Properties

        public Ticket? Ticket { get; set; }

        public BTUser? User { get; set; }

    }
}
