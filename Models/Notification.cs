using System.ComponentModel.DataAnnotations;

namespace BWBugTracker.Models
{
    public class Notification
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }
        
        public int TicketId { get; set; }

        [Required]
        public string? Title { get; set; }
        
        [Required]
        public string? Message { get; set; }

        [DataType(DataType.Date)]
        public DateTime Created { get; set; }

        [Required]
        public virtual string? SenderId { get; set; }
        
        [Required]
        public virtual string? RecipientId { get; set; }

        public int NotificationTypeId { get; set; }

        public bool HasBeenViewed { get; set; }


        // nav props

        public NotificationType? NotificationType { get; set; }

        public Ticket? Ticket { get; set; }

        public Project? Project { get; set; }
        
        public BTUser? Sender { get; set; }
        
        public BTUser? Recipient { get; set; }

    }
}
