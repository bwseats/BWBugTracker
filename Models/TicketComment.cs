using System.ComponentModel.DataAnnotations;

namespace BWBugTracker.Models
{
    public class TicketComment
    {
        public int Id { get; set; }

        [Required]
        public string? Comment { get; set; }

        [DataType(DataType.Date)]
        public DateTime Created { get; set; }

        public int TicketId { get; set; }

        public string? BTUserId { get; set; }


        // nav props

        public virtual Ticket? Ticket { get; set; }

        public virtual BTUser? BTUser { get; set; }
    }
}
