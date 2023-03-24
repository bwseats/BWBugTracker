using BWBugTracker.Data;
using BWBugTracker.Models;
using BWBugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;

namespace BWBugTracker.Services
{
    public class BTTicketService : IBTTicketService
    {
        private readonly ApplicationDbContext _context;

        public BTTicketService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddTicketAsync(Ticket ticket)
        {
            try
            {
                _context.Add(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Ticket> GetTicketAsync(int? ticketId, int? companyId)
        {
			try
			{
				Ticket? ticket = await _context.Tickets
											   .Include(t => t.Project)
											   .Include(t => t.SubmitterUser)
											   .Include(t => t.DeveloperUser)
											   .Include(t => t.Comments)
											   .Include(t => t.TicketPriority)
											   .Include(t => t.TicketStatus)
											   .Include(t => t.TicketType)
											   .Include(t => t.History)
											   .Include(t => t.Attachments)
											   .FirstOrDefaultAsync(m => m.Id == ticketId && m.SubmitterUser.CompanyId == companyId);

				return ticket!;
			}
			catch (Exception)
			{

				throw;
			}
		}

		public async Task<IEnumerable<Ticket>> GetTicketsAsync(int companyId)
		{
			try
			{
				IEnumerable<Ticket> tickets = await _context.Tickets
													   .Where(t => t.Archived == false && t.Project!.CompanyId == companyId)
													   .Include(t => t.DeveloperUser)
													   .Include(t => t.Project)
													   .Include(t => t.SubmitterUser)
													   .Include(t => t.TicketPriority)
													   .Include(t => t.TicketStatus)
													   .Include(t => t.TicketType)
													   .ToListAsync();

				return tickets;
			}
			catch (Exception)
			{

				throw;
			}
		}
        
        public async Task<IEnumerable<Ticket>> GetRecentTicketsAsync(int companyId)
		{
			try
			{
				IEnumerable<Ticket> tickets = await _context.Tickets
													   .Where(t => t.Archived == false && t.Project!.CompanyId == companyId)
													   .Include(t => t.DeveloperUser)
													   .Include(t => t.Project)
													   .Include(t => t.SubmitterUser)
													   .Include(t => t.TicketPriority)
													   .Include(t => t.TicketStatus)
													   .Include(t => t.TicketType)
													   .OrderByDescending(t => t.Created)
													   .ToListAsync();

				return tickets;
			}
			catch (Exception)
			{

				throw;
			}
		}

		public async Task<Ticket> GetTicketAsNoTrackingAsync(int? ticketId, int? companyId)
        {
            try
            {
				Ticket? ticket = await _context.Tickets
								   .Include(t => t.Project)
									   .ThenInclude(p => p.Company)
								   .Include(t => t.Attachments)
								   .Include(t => t.Comments)
								   .Include(t => t.DeveloperUser)
								   .Include(t => t.History)
								   .Include(t => t.SubmitterUser)
								   .Include(t => t.TicketPriority)
								   .Include(t => t.TicketStatus)
								   .Include(t => t.TicketType)
				                   .AsNoTracking()
								   .FirstOrDefaultAsync(t => t.Id == ticketId && t.Project!.CompanyId == companyId && t.Archived == false);

				return ticket!;
			}
            catch (Exception)
            {

                throw;
            }
        }

        public async Task UpdateTicketAsync(Ticket ticket)
        {
            try
            {
                _context.Update(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task ArchiveTicketAsync(Ticket ticket)
        {
            throw new NotImplementedException();
        }

        public async Task AddTicketCommentAsync(TicketComment ticketComment)
        {
            try
            {
                _context.Add(ticketComment);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
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

        public async Task<IEnumerable<Ticket>> GetUserTickets(string? userId)
        {
            try
            {
                IEnumerable<Ticket> tickets = await _context.Tickets
                                                                .Where(t => t.DeveloperUserId == userId || t.SubmitterUserId == userId)
                                                                .Include(t => t.TicketPriority)
                                                                .Include(t => t.TicketStatus)
                                                                .Include(t => t.TicketType)
                                                                .Include(t => t.Comments)
                                                                .Include(t => t.History)
                                                                .Include(t => t.Project)
                                                                .ToListAsync();

                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }
        
        public async Task<IEnumerable<Ticket>> GetRecentUserTickets(string? userId)
        {
            try
            {
                IEnumerable<Ticket> tickets = await _context.Tickets
                                                                .Where(t => t.DeveloperUserId == userId || t.SubmitterUserId == userId)
                                                                .Include(t => t.TicketPriority)
                                                                .Include(t => t.TicketStatus)
                                                                .Include(t => t.TicketType)
                                                                .Include(t => t.Comments)
                                                                .Include(t => t.History)
                                                                .Include(t => t.Project)
																.OrderByDescending(t => t.Created)
																.ToListAsync();

                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
