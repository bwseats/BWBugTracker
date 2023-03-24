using BWBugTracker.Models;

namespace BWBugTracker.Services.Interfaces
{
    public interface IBTTicketService
    {
        #region Ticket CRUD

        public Task AddTicketAsync(Ticket ticket);

        public Task<Ticket> GetTicketAsync(int? ticketId, int? companyId);

        public Task<IEnumerable<Ticket>> GetTicketsAsync(int companyId);

        public Task<IEnumerable<Ticket>> GetRecentTicketsAsync(int companyId);

		public Task<Ticket> GetTicketAsNoTrackingAsync(int? ticketId, int? companyId);

        public Task UpdateTicketAsync(Ticket ticket);

        public Task ArchiveTicketAsync(Ticket ticket);
        #endregion

        #region Extended Ticket Methods

        public Task AddTicketAttachmentAsync(TicketAttachment ticketAttachment);

        #endregion



        public Task<Ticket> GetTicketByIdAsync(int? ticketId);
        
        public Task AddTicketCommentAsync(TicketComment ticketComment);

        public Task<TicketAttachment> GetTicketAttachmentByIdAsync(int ticketAttachmentId);

        //public Task GetTicketAsNoTrackingAsync(ticket.Id, companyId);

        public Task<IEnumerable<Ticket>> GetUserTickets(string? userId);

        public Task<IEnumerable<Ticket>> GetRecentUserTickets(string? userId);
    }
}
