﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BWBugTracker.Data;
using BWBugTracker.Models;
using Microsoft.AspNetCore.Identity;
using BWBugTracker.Services.Interfaces;
using Org.BouncyCastle.Bcpg;
using Microsoft.AspNetCore.Authorization;
using BWBugTracker.Extensions;
using X.PagedList;
using BWBugTracker.Models.Enums;
using BWBugTracker.Models.ViewModels;
using System.Collections;
using BWBugTracker.Services;

namespace BWBugTracker.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTFileService _btFileService;
        private readonly IBTProjectService _projectService;
        private readonly IBTRolesService _rolesService;

        public ProjectsController(UserManager<BTUser> userManager,
                                  IBTFileService btFileService,
                                  IBTProjectService projectService,
                                  IBTRolesService rolesService)
        {
            _userManager = userManager;
            _btFileService = btFileService;
            _projectService = projectService;
            _rolesService = rolesService;
        }

        [HttpGet]
        [Authorize(Roles="Admin")]
        public async Task<IActionResult> AssignPM(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();

            IEnumerable<BTUser> projectManagers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId);
            BTUser? currentPM = await _projectService.GetProjectManagerAsync(id);
            AssignPMViewModel viewModel = new()
            {
                Project = await _projectService.GetProjectByIdAsync(id, companyId),
                PMList = new SelectList(projectManagers, "Id", "FullName", currentPM?.Id),
                PMId = currentPM?.Id
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignPM(AssignPMViewModel viewModel)
        {
            if (!string.IsNullOrEmpty(viewModel.PMId))
            {
                await _projectService.AddProjectManagerAsync(viewModel.PMId, viewModel.Project?.Id);

                return RedirectToAction("Details", new { id = viewModel.Project?.Id });
            }

            ModelState.AddModelError("PMId", "No project manager was found for this project. Please select a new project manager.");
            int companyId = User.Identity!.GetCompanyId();

            IEnumerable<BTUser> projectManagers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId);
            BTUser? currentPM = await _projectService.GetProjectManagerAsync(viewModel.Project?.Id);
            viewModel.Project = await _projectService.GetProjectByIdAsync(viewModel.Project?.Id, companyId);
            viewModel.PMList = new SelectList(projectManagers, "Id", "FullName", currentPM.Id);
            viewModel.PMId = currentPM?.Id;

            return View(viewModel);
        }

        // GET: Projects
        public async Task<IActionResult> Index(int? pageNum)
        {
            int pageSize = 10;
            int page = pageNum ?? 1;

            int companyId = User.Identity!.GetCompanyId();

            IPagedList<Project> projects = (await _projectService.GetProjectsAsync(companyId)).ToPagedList(page, pageSize);

            return View(projects);
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _projectService.GetProjectsAsync == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();

            Project? project = await _projectService.GetProjectAsync(companyId, id.Value);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        public async Task<IActionResult> MyProjects()
        {
            return View();
        }

        public IActionResult PortoDetails()
        {
            return View();
        }

        // GET: Projects/Create
        public async Task<IActionResult> Create()
        {

            IEnumerable<ProjectPriority> priorities = await _projectService.GetProjectPrioritiesAsync();

            ViewData["ProjectPriorityId"] = new SelectList(priorities, "Id", "Name");

            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Created,StartDate,EndDate,ProjectPriorityId,ImageFormFile,ImageFileData,ImageFileType,Archived,CompanyId")] Project project)
        {
            ModelState.Remove("CompanyId");

            if (ModelState.IsValid)
            {
                BTUser? btUser = await _userManager.GetUserAsync(User);

                project.CompanyId = btUser!.CompanyId;

                project.Created = DataUtility.GetPostGresDate(DateTime.UtcNow);
                project.StartDate = DataUtility.GetPostGresDate(project.StartDate);
                project.EndDate = DataUtility.GetPostGresDate(project.EndDate);

                if (project.ImageFormFile != null)
                {
                    project.ImageFileData = await _btFileService.ConvertFileToByteArrayAsync(project.ImageFormFile);
                    project.ImageFileType = project.ImageFormFile.ContentType;
                }

                await _projectService.AddProjectAsync(project);
                return RedirectToAction(nameof(Index));
            }

            IEnumerable<ProjectPriority> priorities = await _projectService.GetProjectPrioritiesAsync();

            ViewData["ProjectPriorityId"] = new SelectList(priorities, "Id", "Name");
            return View(project);
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();

            Project? project = await _projectService.GetProjectAsync(companyId, id.Value);

            if (project == null)
            {
                return NotFound();
            }

            IEnumerable<ProjectPriority> priorities = await _projectService.GetProjectPrioritiesAsync();

            ViewData["ProjectPriorityId"] = new SelectList(priorities, "Id", "Name");
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyId,Name,Description,Created,StartDate,EndDate,ProjectPriorityId,ImageFormFile,ImageFileData,ImageFileType,Archived")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    project.Created = DataUtility.GetPostGresDate(DateTime.UtcNow);
                    project.StartDate = DataUtility.GetPostGresDate(project.StartDate);
                    project.EndDate = DataUtility.GetPostGresDate(project.EndDate);

                    if (project.ImageFormFile != null)
                    {
                        project.ImageFileData = await _btFileService.ConvertFileToByteArrayAsync(project.ImageFormFile);
                        project.ImageFileType = project.ImageFormFile.ContentType;
                    }

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_projectService.ProjectExists(project.Id))
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

            IEnumerable<ProjectPriority> priorities = await _projectService.GetProjectPrioritiesAsync();

            ViewData["ProjectPriorityId"] = new SelectList(priorities, "Id", "Name");
            return View(project);
        }

        // GET: Projects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                int companyId = User.Identity!.GetCompanyId();

                Project project = await _projectService.GetProjectAsync(id!.Value, companyId);

                await _projectService.ArchiveProjectAsync(project);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}