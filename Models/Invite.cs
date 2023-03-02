using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace CSBugTracker.Models
{
    public class Invite
    {
        public int Id { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime InviteDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? JoinDate { get; set; }

        public Guid CompanyToken { get; set; }

        [Required]
        [Display(Name = "Invitee Email")]
        public string? InviteeEmail { get; set; }

        [Required]
        [Display(Name = "Invitee First Name")]
        public string? InviteeFirstName { get; set; }

        [Required]
        [Display(Name = "Invitee Last Name")]
        public string? InviteeLastName { get; set; }


        public string? Message { get; set; }

        public Boolean IsValid { get; set; }


        // Foreign Keys

        public int CompanyId { get; set; }
        public int ProjectId { get; set; }

        [Required]
        public string? InvitorId { get; set; }

        public string? InviteeId { get; set; }


        // Navigation Properties
        public virtual Company? Company { get; set; }
        public virtual Project? Project { get; set; }
        public virtual BTUser? Invitor { get; set; }
        public virtual BTUser? Invitee { get; set; }

    }
}
