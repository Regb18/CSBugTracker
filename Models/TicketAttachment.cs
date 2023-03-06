using CSBugTracker.Extensions;
using System.ComponentModel;
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


        public string? FileName { get; set; }
        public byte[]? FileData { get; set; }
        public string? FileContentType { get; set; }


        [NotMapped]
        [DisplayName("Select a file")]
        [DataType(DataType.Upload)]
        [MaxFileSize(1024 * 1024)]
        [AllowedExtensions(new string[] { ".jpg", ".png", ".doc", ".docx", ".xls", ".xlsx", ".pdf" })]
        public IFormFile? FormFile { get; set; }

        // Foreign Keys

        public int TicketId { get; set; }

        [Required]
        public string? UserId { get; set; }

        // Navigation Properties

        public Ticket? Ticket { get; set; }

        public BTUser? User { get; set; }

    }
}
