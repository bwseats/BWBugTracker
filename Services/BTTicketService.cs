using BWBugTracker.Data;
using BWBugTracker.Models;
using BWBugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BWBugTracker.Services
{
    public class BTTicketService : IBTTicketService
    {
        private readonly ApplicationDbContext _context;

        public BTTicketService(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task AddTicketAsync(Ticket ticket)
        {
            throw new NotImplementedException();
        }
        
        public Task<Ticket> GetTicketAsync(int ticketId)
        {
            throw new NotImplementedException();
        }
        
        public Task UpdateTicketAsync(Ticket ticket)
        {
            throw new NotImplementedException();
        }

        public Task ArchiveTicketAsync(Ticket ticket)
        {
            throw new NotImplementedException();
        }

        public async Task AddTicketAttachmentAsync(TicketAttachment ticketAttachment)
        {
            try
            {
                await _context.AddAsync(ticketAttachment);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
