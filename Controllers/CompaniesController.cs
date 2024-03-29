﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BWBugTracker.Data;
using BWBugTracker.Models;
using BWBugTracker.Models.ViewModels;
using BWBugTracker.Extensions;
using Microsoft.AspNetCore.Identity;
using BWBugTracker.Services;
using BWBugTracker.Services.Interfaces;
using BWBugTracker.Models.Enums;
using Microsoft.AspNetCore.Authorization;

namespace BWBugTracker.Controllers
{
	[Authorize(Roles = "Admin, ProjectManager")]
	public class CompaniesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTRolesService _rolesService;
        private readonly IBTCompanyService _companyService;

        public CompaniesController(ApplicationDbContext context,
                                   UserManager<BTUser> userManager,
                                   IBTRolesService rolesService,
                                   IBTCompanyService companyService)
        {
            _context = context;
            _userManager = userManager;
            _rolesService = rolesService;
            _companyService = companyService;
        }

		// GET: Companies
		public async Task<IActionResult> Index()
		{
			IEnumerable<Company> companies = await _companyService.GetRecentCompaniesAsync();

			return View(companies);
		}

		// GET: Companies/Details/5
		public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Companies == null)
            {
                return NotFound();
            }

            var company = await _context.Companies
                                        .FirstOrDefaultAsync(m => m.Id == id);

            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        // GET: Companies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Companies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,ImageFileData,ImageFileType")] Company company)
        {
            if (ModelState.IsValid)
            {
                _context.Add(company);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(company);
        }

        // GET: Companies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Companies == null)
            {
                return NotFound();
            }

            var company = await _context.Companies.FindAsync(id);

            if (company == null)
            {
                return NotFound();
            }
            return View(company);
        }

        // POST: Companies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,ImageFileData,ImageFileType")] Company company)
        {
            if (id != company.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(company);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompanyExists(company.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(company);
        }

        // GET: Companies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Companies == null)
            {
                return NotFound();
            }

            var company = await _context.Companies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        // POST: Companies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Companies == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Companies'  is null.");
            }
            var company = await _context.Companies.FindAsync(id);
            if (company != null)
            {
                _context.Companies.Remove(company);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CompanyExists(int id)
        {
          return (_context.Companies?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        [HttpGet]
        public async Task<IActionResult> ManageUserRoles()
        {
            // add an instance of the viewmodel as a list (model)
            List<ManageUserRolesViewModel> viewModelList = new List<ManageUserRolesViewModel>();

            // get CompanyId
            int companyId = User.Identity!.GetCompanyId();

            // get all company users
            IEnumerable<BTUser>? btUsers = await _companyService.GetMembersAsync(companyId);

			// loop over the users to populate an instance of the viewmodel
			foreach (BTUser btUser in btUsers)
            {
				IEnumerable<string> currentRoles = await _rolesService.GetUserRolesAsync(btUser);

				// instantiate single viewModel
				ManageUserRolesViewModel viewModel = new()
				{
					BTUser = btUser,

					// use _rolesService to help populate the viewmodel instance 
					// create multiselect
					Roles = new MultiSelectList(await _rolesService.GetRolesAsync(), "Name", "Name", currentRoles)
				};

				// add viewModel to model list
				viewModelList.Add(viewModel);

			}

            // return the model to the view
            return View(viewModelList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManagerUserRoles(ManageUserRolesViewModel viewModel)
        {
            // get the CompanyId
            int companyId = User.Identity!.GetCompanyId();

            // instantiate the BTUser
            BTUser? btUser = await _companyService.GetMemberAsync(viewModel.BTUser!.Id, companyId);

            // get roles for the user
            IEnumerable<string> currentRoles = await _rolesService.GetUserRolesAsync(btUser);

            // get selected role(s) for the user submitted from the form
            IEnumerable<string> selectedRoles = viewModel.SelectedRoles!.ToList();

            // remove current role(s) and add new role
            await _rolesService.RemoveUserFromRolesAsync(btUser!, currentRoles);

            foreach (string role in selectedRoles)
            {
                await _rolesService.AddUserToRoleAsync(btUser!, role);
            }

            await _context.SaveChangesAsync();

            // navigate
            return RedirectToAction("ManageUserRoles");

        }
    }
}
