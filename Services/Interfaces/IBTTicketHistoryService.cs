using BWBugTracker.Models;

namespace BWBugTracker.Services.Interfaces
{
    public interface IBTTicketHistoryService
    {
        Task AddHistoryAsync(Ticket? oldticket, Ticket? newTicket, string? userId);

        Task AddHistoryAsync(int? ticketId, string? model, string? userId);

        Task<List<TicketHistory>> GetProjectTicketHistoriesAsync(int? projectId, int? companyId);
        Task<List<TicketHistory>> GetCompanyTicketHistoriesAsync(int? companyId);
    }
}
