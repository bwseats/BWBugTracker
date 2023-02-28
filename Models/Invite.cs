using System.ComponentModel.DataAnnotations;

namespace BWBugTracker.Models
{
    public class Invite
    {
        public int Id { get; set; }

        [DataType(DataType.Date)]
        public DateTime InviteDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? JoinDate { get; set; }

        public Guid CompanyToken { get; set; }

        public int CompanyId { get; set; }

        public int ProjectId { get; set; }

        [Required]
        public string? InvitorId { get; set; }
        
        public string? InviteeId { get; set; }

        [Required]
        public string? InviteeEmail { get; set; }
        
        [Required]
        [Display(Name = "First Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 1)]
        public string? InviteeFirstName { get; set; }
        
        [Required]
        [Display(Name = "First Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 1)]
        public string? InviteeLastName { get; set; }

        public string? Message { get; set; }

        public bool IsValid { get; set; }


        // nav props

        public Company? Company { get; set; }

        public Project? Project { get; set; }

        public BTUser? Invitor { get; set; }

        public BTUser? Invitee { get; set; }

    }
}
