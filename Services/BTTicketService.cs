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
        
        public async Task<Ticket> GetTicketAsync(int? ticketId)
        {
            var ticket = await _context.Tickets
                                       .Include(t => t.DeveloperUser)
                                       .Include(t => t.Project)
                                       .Include(t => t.SubmitterUser)
                                       .Include(t => t.Comments)
                                       .Include(t => t.TicketPriority)
                                       .Include(t => t.TicketStatus)
                                       .Include(t => t.TicketType)
                                       .FirstOrDefaultAsync(m => m.Id == ticketId);

            return ticket!;
        }
        
        public async Task<Ticket> GetTicketAsNoTrackingAsync(int? ticketId)
        {
            var ticket = await _context.Tickets
                                       .Include(t => t.DeveloperUser)
                                       .Include(t => t.Project)
                                       .Include(t => t.SubmitterUser)
                                       .Include(t => t.Comments)
                                       .Include(t => t.TicketPriority)
                                       .Include(t => t.TicketStatus)
                                       .Include(t => t.TicketType)
                                       .AsNoTracking()
                                       .FirstOrDefaultAsync(m => m.Id == ticketId);

            return ticket!;
        }
        
        public async Task UpdateTicketAsync(Ticket ticket)
        {
            _context.Update(ticket);
            await _context.SaveChangesAsync();
        }

        public Task ArchiveTicketAsync(Ticket ticket)
        {
            throw new NotImplementedException();
        }

        public async Task AddTicketCommentAsync(TicketComment ticketComment)
        {
            _context.Add(ticketComment);
            await _context.SaveChangesAsync();
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

        public async Task<Ticket> GetTicketByIdAsync(int? ticketId)
        {


            try
            {
                Ticket? ticket = await _context.Tickets
                                               .Include(t => t.DeveloperUser)
                                               .Include(t => t.Project)
                                               .Include(t => t.SubmitterUser)
                                               .Include(t => t.TicketPriority)
                                               .Include(t => t.TicketStatus)
                                               .Include(t => t.TicketType)
                                               .Include(t => t.Comments)
                                               .Include(t => t.Attachments)
                                               .Include(t => t.History)
                                               .FirstOrDefaultAsync(t => t.Id == ticketId);


                return ticket!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<TicketAttachment> GetTicketAttachmentByIdAsync(int ticketAttachmentId)
        {
            try
            {
                TicketAttachment? ticketAttachment = await _context.TicketAttachments
                                                                  .Include(t => t.BTUser)
                                                                  .FirstOrDefaultAsync(t => t.Id == ticketAttachmentId);
                return ticketAttachment!;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
