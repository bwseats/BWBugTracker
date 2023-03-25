using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BWBugTracker.Data;
using BWBugTracker.Models;
using Microsoft.AspNetCore.Identity;
using BWBugTracker.Services;
using BWBugTracker.Services.Interfaces;
using BWBugTracker.Extensions;
using BWBugTracker.Models.Enums;
using BWBugTracker.Models.ViewModels;
using X.PagedList;
using System.ComponentModel.Design;
using Microsoft.AspNetCore.Authorization;

namespace BWBugTracker.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTFileService _btFileService;
        private readonly IBTTicketService _btTicketService;
        private readonly IBTRolesService _btRolesService;
        private readonly IBTTicketHistoryService _btHistoryService;
        private readonly IBTProjectService _btProjectService;
        private readonly IBTNotificationService _btNotificationService;

        public TicketsController(ApplicationDbContext context,
                                 UserManager<BTUser> userManager,
                                 IBTFileService btFileService,
                                 IBTTicketService btTicketService,
                                 IBTRolesService btRolesService,
                                 IBTTicketHistoryService btHistoryService,
                                 IBTProjectService btProjectService,
                                 IBTNotificationService btNotificationService)
        {
            _context = context;
            _userManager = userManager;
            _btFileService = btFileService;
            _btTicketService = btTicketService;
            _btRolesService = btRolesService;
            _btHistoryService = btHistoryService;
            _btProjectService = btProjectService;
            _btNotificationService = btNotificationService;
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {
			int companyId = User.Identity!.GetCompanyId();

			IEnumerable<Ticket> tickets = await _btTicketService.GetTicketsAsync(companyId);

			return View(tickets);
		}
        
        public async Task<IActionResult> IndexCopy()
        {
			int companyId = User.Identity!.GetCompanyId();

			IEnumerable<Ticket> tickets = await _btTicketService.GetTicketsAsync(companyId);

			return View(tickets);
		}
        
        public async Task<IActionResult> PortoIndex()
        {
			int companyId = User.Identity!.GetCompanyId();

			IEnumerable<Ticket> tickets = await _btTicketService.GetTicketsAsync(companyId);

			return View(tickets);
		}

        public async Task<IActionResult> MyTickets()
        {
            string? userId = _userManager.GetUserId(User);

            IEnumerable<Ticket> tickets = await _btTicketService.GetRecentUserTickets(userId);

            return View(tickets);
        }

        public async Task<IActionResult> PortoDetails(int? id)
        {
            int companyId = User.Identity!.GetCompanyId();

			if (id == null)
			{
				return NotFound();
			}

			Ticket ticket = await _btTicketService.GetTicketAsync(id, companyId);

			if (ticket == null)
			{
				return NotFound();
			}

			return View(ticket);
		}

   //     // GET: Tickets/Details/5
   //     public async Task<IActionResult> Details(int? id)
   //     {
   //         if (id == null || _context.Tickets == null)
   //         {
   //             return NotFound();
   //         }

			//Ticket ticket = await _btTicketService.GetTicketAsync(id);

			//if (ticket == null)
   //         {
   //             return NotFound();
   //         }

   //         return View(ticket);
   //     }

        // GET: Tickets/Create
        public async Task<IActionResult> Create()
        {
            BTUser? btUser = await _userManager.GetUserAsync(User);

            IEnumerable<Project> projects = _context.Projects.Where(p => p.CompanyId == btUser!.CompanyId);

            ViewData["ProjectId"] = new SelectList(projects, "Id", "Name");
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name");
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Created,Updated,Archived,ArchivedByProject,ProjectId,TicketTypeId,TicketStatusId,TicketPriorityId,DeveloperUserId,SubmitterUserId")] Ticket ticket)
        {
            ModelState.Remove("SubmitterUserId");

            BTUser? btUser = await _userManager.GetUserAsync(User);

            if (ModelState.IsValid)
            {
                string? userId = _userManager.GetUserId(User);

                ticket.SubmitterUserId = userId;

                ticket.Created = DataUtility.GetPostGresDate(DateTime.UtcNow);

                ticket.Updated = DataUtility.GetPostGresDate(DateTime.UtcNow);

                ticket.TicketStatusId = (await _context.TicketStatuses.FirstOrDefaultAsync(s => s.Name == nameof(BTTicketStatuses.New)))!.Id;

                await _btTicketService.AddTicketAsync(ticket);

                //TODO: add history record
                int companyId = User.Identity!.GetCompanyId();

                Ticket? newTicket = await _btTicketService.GetTicketAsNoTrackingAsync(ticket.Id, companyId);

                await _btHistoryService.AddHistoryAsync(null, newTicket, userId);


                //TODO: notification

                BTUser? projectManager = await _btProjectService.GetProjectManagerAsync(ticket.ProjectId);

                Notification? notification = new()
                {
                    TicketId = ticket.Id,
                    Title = "A new ticket has been added",
                    Message = $"Ticket '{ticket.Title}' was created by {btUser?.FullName}.",
                    Created = DataUtility.GetPostGresDate(DateTime.Now),
                    ProjectId = ticket.ProjectId,
                    SenderId = userId,
                    RecipientId = projectManager?.Id,
                    NotificationTypeId = (await _context.NotificationTypes.FirstOrDefaultAsync(n => n.Name == nameof(BTNotificationTypes.Ticket)))!.Id
                };

                if (projectManager != null)
                {
                    await _btNotificationService.AddNotificationAsync(notification);
                    await _btNotificationService.SendEmailNotificationAsync(notification, "New Ticket Added");
                }
                else
                {
                    await _btNotificationService.AdminNotificationAsync(notification, companyId);
                    await _btNotificationService.SendAdminEmailNotificationAsync(notification, "New Project Ticket Added", companyId);
                }

                return RedirectToAction("PortoIndex", "Home");
            }

            ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.DeveloperUserId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Id", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Id", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Id", ticket.TicketTypeId);

            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets.FindAsync(id);

            if (ticket == null)
            {
                return NotFound();
            }

            ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "FullName", ticket.DeveloperUserId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);

            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Created,Updated,Archived,ArchivedByProject,ProjectId,TicketTypeId,TicketStatusId,TicketPriorityId,DeveloperUserId,SubmitterUserId")] Ticket ticket)
        {
            int companyId = User.Identity!.GetCompanyId();

            if (id != ticket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                string? userId = _userManager.GetUserId(User);
                
                Ticket oldTicket = await _btTicketService.GetTicketAsNoTrackingAsync(id, companyId);

                try
                {

                    ticket.Created = DataUtility.GetPostGresDate(ticket.Created);
                    ticket.Updated = DataUtility.GetPostGresDate(DateTime.UtcNow);

                    await _btTicketService.UpdateTicketAsync(ticket);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                Ticket newTicket = await _btTicketService.GetTicketAsNoTrackingAsync(id, companyId);

                await _btHistoryService.AddHistoryAsync(oldTicket, newTicket, userId);

                // notifcation

                BTUser? btUser = await _userManager.GetUserAsync(User);

                BTUser? projectManager = await _btProjectService.GetProjectManagerAsync(ticket.ProjectId);

                Notification? notification = new()
                {
                    TicketId = ticket.Id,
                    Title = "A new ticket has been added",
                    Message = $"Ticket '{ticket.Title}' was created by {btUser?.FullName}.",
                    Created = DataUtility.GetPostGresDate(DateTime.Now),
                    SenderId = userId,
                    RecipientId = projectManager?.Id,
                    NotificationTypeId = (await _context.NotificationTypes.FirstOrDefaultAsync(n => n.Name == nameof(BTNotificationTypes.Ticket)))!.Id
                };

                //if (projectManager != null)
                //{
                //    await _btNotificationService.AddNotificationAsync(notification);
                //    await _btNotificationService.SendEmailNotificationAsync(notification, "New Ticket Added");
                //}
                //else
                //{
                //    await _btNotificationService.AdminNotificationAsync(notification, companyId);
                //    await _btNotificationService.SendAdminEmailNotificationAsync(notification, "New Project Ticket Added", companyId);
                //}

                return RedirectToAction(nameof(PortoDetails));
            }

            ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Name", ticket.DeveloperUserId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);

            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            int companyId = User.Identity!.GetCompanyId();

            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

			Ticket ticket = await _btTicketService.GetTicketAsync(id, companyId);

			if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            int companyId = User.Identity!.GetCompanyId();

            if (_context.Tickets == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Tickets'  is null.");
            }

			Ticket ticket = await _btTicketService.GetTicketAsync(id, companyId);

			if (ticket != null)
            {
                ticket.Archived = true;
            }

			await _btTicketService.UpdateTicketAsync(ticket!);

			return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketComment([Bind("Id,TicketId,BTUserId,Comment,Created")] TicketComment ticketComment)
        {

            int ticketId = ticketComment.TicketId;

            ModelState.Remove("BTUserId");

            if (ModelState.IsValid)
            {
                ticketComment.BTUserId = _userManager.GetUserId(User);

                ticketComment.Created = DataUtility.GetPostGresDate(DateTime.UtcNow);

                await _btTicketService.AddTicketCommentAsync(ticketComment);

                await _btHistoryService.AddHistoryAsync(ticketComment.TicketId, nameof(TicketComment), ticketComment.BTUserId);

                return RedirectToAction("PortoDetails", new { id = ticketId });
            }

            return RedirectToAction("PortoDetails", new { id = ticketId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketAttachment([Bind("Id,FormFile,Description,TicketId")] TicketAttachment ticketAttachment)
        {
            string statusMessage;

            ModelState.Remove("BTUserId");

            if (ModelState.IsValid && ticketAttachment.FormFile != null)
            {
                try
                {
					ticketAttachment.FileData = await _btFileService.ConvertFileToByteArrayAsync(ticketAttachment.FormFile);
					ticketAttachment.FileName = ticketAttachment.FormFile.FileName;
					ticketAttachment.FileType = ticketAttachment.FormFile.ContentType;

					ticketAttachment.Created = DataUtility.GetPostGresDate(DateTime.UtcNow);
					ticketAttachment.BTUserId = _userManager.GetUserId(User);

					await _btTicketService.AddTicketAttachmentAsync(ticketAttachment);

                    await _btHistoryService.AddHistoryAsync(ticketAttachment.TicketId, nameof(TicketAttachment), ticketAttachment.BTUserId);

                }
                catch (Exception)
                {

                    throw;
                }

                statusMessage = "Success: New attachment added to Ticket.";
            }
            else
            {
                statusMessage = "Error: Invalid data.";

            }

            return RedirectToAction("PortoDetails", new { id = ticketAttachment.TicketId, message = statusMessage });
        }

		public async Task<IActionResult> ShowFile(int id)
		{
			TicketAttachment ticketAttachment = await _btTicketService.GetTicketAttachmentByIdAsync(id);
			string fileName = ticketAttachment.FileName!;
			byte[] fileData = ticketAttachment.FileData!;
			string ext = Path.GetExtension(fileName).Replace(".", "");

			Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
			return File(fileData, $"application/{ext}");
		}

		private bool TicketExists(int id)
        {
            return (_context.Tickets?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        [HttpGet]
        public async Task<IActionResult> AssignTicketDeveloper(int? ticketId)
        {
            if (ticketId == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();

            Ticket? ticket = await _btTicketService.GetTicketByIdAsync(ticketId);

            List<BTUser> DeveloperList = await _btRolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId);

            string? currentDeveloperId = ticket.DeveloperUserId;

            AssignDeveloperViewModel viewModel = new()
            {
                Ticket = ticket,
                DeveloperList = new SelectList(DeveloperList, "Id", "FullName", currentDeveloperId)
            };

            return View(viewModel);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTicketDeveloper(AssignDeveloperViewModel model)
        {
            int companyId = User.Identity!.GetCompanyId();

            Ticket? ticket = await _btTicketService.GetTicketAsync(model.Ticket?.Id, companyId);

            
            string? userId = _userManager.GetUserId(User);


            if (model.DeveloperId != null)
            {
                Ticket oldTicket = await _btTicketService.GetTicketAsNoTrackingAsync(model.Ticket?.Id, companyId);

                try
                {
                    ticket.DeveloperUserId = model.DeveloperId;

                    await _btTicketService.UpdateTicketAsync(ticket);
                }
                catch (Exception)
                {

                    throw;
                }

                Ticket newTicket = await _btTicketService.GetTicketAsNoTrackingAsync(model.Ticket?.Id, companyId);

                await _btHistoryService.AddHistoryAsync(oldTicket, newTicket, userId);

                // notification

                BTUser? btUser = await _userManager.GetUserAsync(User);

                Notification? notification = new()
                {
                    TicketId = model.Ticket!.Id,
                    Title = "A new developer has been assigned",
                    Message = $"Ticket '{model.Ticket.Title}' was assigned by {btUser?.FullName}.",
                    Created = DataUtility.GetPostGresDate(DateTime.Now),
                    SenderId = userId,
                    RecipientId = model.DeveloperId,
                    NotificationTypeId = (await _context.NotificationTypes.FirstOrDefaultAsync(n => n.Name == nameof(BTNotificationTypes.Ticket)))!.Id
                };
                
                await _btNotificationService.AddNotificationAsync(notification);
                await _btNotificationService.SendEmailNotificationAsync(notification, "New Developer Assigned");

                return RedirectToAction("Index");
            }

            List<BTUser> developers = await _btRolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId);

            string? currentDeveloperId = model.DeveloperId;

            AssignDeveloperViewModel viewModel = new()
            {
                Ticket = ticket,
                DeveloperList = new SelectList(developers, "Id", "FullName", currentDeveloperId)
            };

            return View(viewModel);
        }
    }
}
