using BWBugTracker.Extensions;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BWBugTracker.Models
{
    public class TicketAttachment
    {
        public int Id { get; set; }

        public string? Description { get; set; }

        [DataType(DataType.Date)]
        public DateTime Created { get; set; }

        public int TicketId { get; set; }

        [Required]
        public string? BTUserId { get; set; }

        [NotMapped]
        [DisplayName("Select a file")]
        [DataType(DataType.Upload)]
        [MaxFileSize(1024 * 1024)]
        [AllowedExtensions(new string[] { ".jpg", ".png", ".doc", ".docx", ".xls", ".xlsx", ".pdf" })]
        public IFormFile? FormFile { get; set; }
        public byte[]? FileData { get; set; }
        public string? FileType { get; set; }
        public string? FileName { get; set; }


        // nav props

        public virtual Ticket? Ticket { get; set; }

        public virtual BTUser? BTUser { get; set; }
    }
}
