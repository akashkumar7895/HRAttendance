using HRAttendanceMVC.Application.Interfaces;
using HRAttendanceMVC.Domain.Enities;
using HRAttendanceMVC.Domain.Entities;
using HRAttendanceMVC.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HRAttendanceMVC.Controllers
{
    public class FooterController : Controller
    {
        // =====================================================
        // SERVICES & DBCONTEXT
        // =====================================================

        private readonly IContactService _contactService;
        private readonly AppDbContext _context;

        public FooterController(IContactService contactService, AppDbContext context)
        {
            _contactService = contactService;
            _context = context;
        }

        private int CurrentUserId =>
            int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

        private async Task<int?> GetEffectiveHrUserIdAsync()
        {
            if (User.IsInRole("HR"))
            {
                if (int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) && id > 0)
                    return id;
            }
            else if (User.IsInRole("Employee"))
            {
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                if (!string.IsNullOrEmpty(email))
                {
                    var emp = await _context.Employees.FirstOrDefaultAsync(e => e.Email.ToLower() == email.ToLower());
                    if (emp != null && emp.HrUserId > 0)
                        return emp.HrUserId;
                }
            }
            else if (CurrentUserId > 0)
            {
                return CurrentUserId;
            }
            return null;
        }


        private IActionResult Page(
            string title,
            string subtitle,
            string description,
            string theme = "purple")
        {
            ViewBag.Title = title;
            ViewBag.Subtitle = subtitle;
            ViewBag.Description = description;
            ViewBag.Theme = theme;
            return View("Page");
        }


        // =========================
        // PRODUCT
        // =========================

        public IActionResult HrSoftware()
        {
            return Page(
                "HR Software",
                "Complete HR Management Solution",
                "Manage your entire workforce from one powerful HR platform. Simplify employee management, attendance, leave and HR operations.",
                 "purple"
            );
        }


        public IActionResult LeaveManagement()
        {
            return Page(
                "Leave Management",
                "Simplify Employee Leave Management",
                "Create leave policies, manage leave requests and track employee leave balances easily from one centralized platform.",
                  "green"
            );
        }


        public IActionResult AttendanceManagement()
        {
            return Page(
                "Attendance Management",
                "Smart Attendance Tracking",
                "Track employee attendance, working hours, late arrivals, absences and attendance reports with ease.",
                  "blue"
            );
        }


        public IActionResult EmployeeEngagement()
        {
            return Page(
                "Employee Engagement",
                "Build a More Connected Workplace",
                "Improve employee engagement with better communication, recognition and workforce management tools.",
                 "pink"
            );
        }


        public IActionResult HrServiceStatus()
        {
            return Page(
                "HR Service Status",
                "System Status",
                "Check the current availability and operational status of HR Attendance services.",
                 "orange"
            );
        }


        // =========================
        // HR & PAYROLL
        // =========================

        public IActionResult WhatIsHrms()
        {
            ViewBag.Title = "What is HRMS?";
            ViewBag.Subtitle = "Complete Guide to Human Resource Management Systems";
            ViewBag.Description = "An HRMS (Human Resource Management System) is an all-in-one software solution that automates employee databases, daily attendance tracking, leave policies, and monthly payroll in one unified platform.";
            ViewBag.Theme = "purple";
            ViewBag.PageType = "whatishrms";
            return View("Page");
        }


        [Route("Resources/GuideLeaveManagement")]
        [Route("Footer/GuideLeaveManagement")]
        [Route("GuideLeaveManagement")]
        public IActionResult GuideLeaveManagement()
        {
            ViewBag.Title = "Guide to Leave Management";
            ViewBag.Subtitle = "Mastering Modern Workforce Leave Policies & Operations";
            ViewBag.Description = "Comprehensive guide for HR professionals and employees to design balanced leave policies, streamline digital leave requests, eliminate tracking errors, and synchronize time-off with monthly payroll.";
            ViewBag.Theme = "blue";
            ViewBag.PageType = "guideleavemanagement";
            return View("Page");
        }


        [Route("Resources/GuideAttendanceManagement")]
        [Route("Footer/GuideAttendanceManagement")]
        [Route("GuideAttendanceManagement")]
        public IActionResult GuideAttendanceManagement()
        {
            ViewBag.Title = "Guide to Attendance Management";
            ViewBag.Subtitle = "Best Practices for Modern Time, Punctuality & Attendance Tracking";
            ViewBag.Description = "Learn how modern organizations monitor daily attendance, track punctuality and working hours, prevent absenteeism, and automate payroll sync with zero manual punch hassles.";
            ViewBag.Theme = "green";
            ViewBag.PageType = "guideattendancemanagement";
            return View("Page");
        }


        // =========================
        // RESOURCES
        // =========================

        public IActionResult Guides()
        {
            return Page(
                "Guides",
                "HR Guides & Learning",
                "Explore practical guides to help your organization manage HR operations more efficiently.",
                  "blue"
            );
        }


        public IActionResult Templates()
        {
            return Page(
                "Templates",
                "Ready-to-Use HR Templates",
                "Use professional HR templates for employees, attendance, leave management and HR processes.",
                     "pink"
            );
        }


        public IActionResult Ebooks()
        {
            return Page(
                "Ebooks",
                "HR Knowledge Library",
                "Discover useful HR resources, guides and knowledge material for modern HR teams.",
                        "orange"
            );
        }


        public IActionResult Podcasts()
        {
            return Page(
                "Podcasts",
                "HR Conversations",
                "Listen to conversations and insights about HR management, employees and workplace transformation.",
                "dark"
            );
        }


        public IActionResult Academy()
        {
            return Page(
                "Academy",
                "Learn HR Management",
                "Build your HR knowledge with structured learning resources and practical HR management concepts.",
                "purple"
            );
        }


        // =========================
        // HR CUSTOMERS
        // =========================

        public IActionResult HrHelp()
        {
            ViewBag.Title = "HR Help & Support Center";
            ViewBag.Subtitle = "Everything you need to manage attendance, employees, leaves, and payroll";
            ViewBag.Description = "Find instant answers, step-by-step guides, troubleshooting tips, and connect with our dedicated HR support team.";
            ViewBag.Theme = "purple";
            ViewBag.PageType = "hrhelp";
            return View("Page");
        }


        public IActionResult Videos()
        {
            ViewBag.Title = "HR Video Tutorials & Demos";
            ViewBag.Subtitle = "Learn HR Management Systems through Interactive Video Walkthroughs";
            ViewBag.Description = "Watch comprehensive video guides explaining how Human Resource Management Systems work, streamline daily attendance, approve leaves, and calculate monthly payroll.";
            ViewBag.Theme = "blue";
            ViewBag.PageType = "videos";
            return View("Page");
        }


        // =========================
        // COMPANY
        // =========================

        [Route("Company/Engage")]
        [Route("Footer/Engage")]
        public async Task<IActionResult> Engage()
        {
            ViewBag.Title = "Engage – Activities, News & Events";
            ViewBag.Subtitle = "Company Announcements, Events Calendar, Celebrations & Team Activities";
            ViewBag.Description = "Stay connected with workplace announcements, holiday calendar, employee birthdays, work anniversaries, and employee engagement activities.";
            ViewBag.Theme = "purple";
            ViewBag.PageType = "engage";

            var hrId = await GetEffectiveHrUserIdAsync();

            var annQuery = _context.CompanyAnnouncements.AsQueryable();
            var evtQuery = _context.CompanyEvents.AsQueryable();
            var celQuery = _context.EmployeeCelebrations.AsQueryable();
            var empQuery = _context.Employees.Where(e => e.IsActive);
            var postQuery = _context.EngagementPosts.AsQueryable();

            if (hrId.HasValue && hrId.Value > 0)
            {
                annQuery = annQuery.Where(a => a.HrUserId == hrId.Value);
                evtQuery = evtQuery.Where(e => e.HrUserId == hrId.Value);
                celQuery = celQuery.Where(c => c.HrUserId == hrId.Value);
                empQuery = empQuery.Where(e => e.HrUserId == hrId.Value);
                postQuery = postQuery.Where(p => p.HrUserId == hrId.Value);
            }

            // 1. Announcements added by HR
            var announcements = await annQuery
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
            ViewBag.Announcements = announcements;

            // 2. Events & Holidays added by HR
            var allEvents = await evtQuery
                .OrderBy(e => e.EventDate)
                .ToListAsync();
            ViewBag.CompanyEvents = allEvents.Where(e => e.EventType != "Holiday").ToList();
            ViewBag.Holidays = allEvents.Where(e => e.EventType == "Holiday").ToList();

            // 3. Employee Celebrations (Birthdays & Anniversaries)
            var celebrations = await celQuery
                .OrderBy(c => c.CelebrationDate)
                .ToListAsync();
            ViewBag.Celebrations = celebrations;

            // Real active employees for auto-anniversaries
            var activeEmployees = await empQuery
                .OrderBy(e => e.FirstName)
                .ToListAsync();
            ViewBag.ActiveEmployees = activeEmployees;

            // 4. Activity Posts
            var posts = await postQuery
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            ViewBag.Posts = posts;

            return View("Page");
        }

        // =====================================================
        // ANNOUNCEMENT ACTIONS (HR)
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAnnouncement(string title, string category, string content)
        {
            if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(content))
            {
                var author = User.Identity?.Name ?? "HR Team";
                var hrId = await GetEffectiveHrUserIdAsync();
                var announcement = new CompanyAnnouncement
                {
                    Title = title.Trim(),
                    Category = string.IsNullOrWhiteSpace(category) ? "General" : category.Trim(),
                    Content = content.Trim(),
                    AuthorName = string.IsNullOrWhiteSpace(author) ? "HR Team" : author.Trim(),
                    CreatedAt = DateTime.Now,
                    HrUserId = (hrId.HasValue && hrId.Value > 0) ? hrId.Value : (CurrentUserId > 0 ? CurrentUserId : 1)
                };
                _context.CompanyAnnouncements.Add(announcement);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Announcement successfully published!";
            }
            return RedirectToAction(nameof(Engage));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAnnouncement(int id)
        {
            var item = await _context.CompanyAnnouncements.FindAsync(id);
            if (item != null)
            {
                _context.CompanyAnnouncements.Remove(item);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Announcement deleted successfully.";
            }
            return RedirectToAction(nameof(Engage));
        }

        // =====================================================
        // EVENT / HOLIDAY ACTIONS (HR)
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEvent(string title, string eventType, string? subType, DateTime eventDate, string? location, string? description)
        {
            if (!string.IsNullOrWhiteSpace(title))
            {
                var hrId = await GetEffectiveHrUserIdAsync();
                var evt = new CompanyEvent
                {
                    Title = title.Trim(),
                    EventType = string.IsNullOrWhiteSpace(eventType) ? "CompanyEvent" : eventType.Trim(),
                    SubType = subType?.Trim(),
                    EventDate = eventDate,
                    Location = location?.Trim(),
                    Description = description?.Trim(),
                    CreatedAt = DateTime.Now,
                    HrUserId = (hrId.HasValue && hrId.Value > 0) ? hrId.Value : (CurrentUserId > 0 ? CurrentUserId : 1)
                };
                _context.CompanyEvents.Add(evt);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Event / Holiday successfully added!";
            }
            return RedirectToAction(nameof(Engage));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var item = await _context.CompanyEvents.FindAsync(id);
            if (item != null)
            {
                _context.CompanyEvents.Remove(item);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Event deleted successfully.";
            }
            return RedirectToAction(nameof(Engage));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleEventRsvp(int id)
        {
            var evt = await _context.CompanyEvents.FindAsync(id);
            if (evt != null)
            {
                evt.RsvpCount += 1;
                await _context.SaveChangesAsync();
                return Json(new { success = true, count = evt.RsvpCount });
            }
            return Json(new { success = false });
        }

        // =====================================================
        // CELEBRATION ACTIONS (HR)
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCelebration(string employeeName, string celebrationType, string? department, string? designation, DateTime celebrationDate, int? yearsOfService)
        {
            if (!string.IsNullOrWhiteSpace(employeeName))
            {
                var hrId = await GetEffectiveHrUserIdAsync();
                var cel = new EmployeeCelebration
                {
                    EmployeeName = employeeName.Trim(),
                    CelebrationType = string.IsNullOrWhiteSpace(celebrationType) ? "Birthday" : celebrationType.Trim(),
                    Department = department?.Trim(),
                    Designation = designation?.Trim(),
                    CelebrationDate = celebrationDate,
                    YearsOfService = yearsOfService,
                    CreatedAt = DateTime.Now,
                    HrUserId = (hrId.HasValue && hrId.Value > 0) ? hrId.Value : (CurrentUserId > 0 ? CurrentUserId : 1)
                };
                _context.EmployeeCelebrations.Add(cel);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Celebration milestone added!";
            }
            return RedirectToAction(nameof(Engage));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCelebration(int id)
        {
            var item = await _context.EmployeeCelebrations.FindAsync(id);
            if (item != null)
            {
                _context.EmployeeCelebrations.Remove(item);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Celebration deleted.";
            }
            return RedirectToAction(nameof(Engage));
        }

        // =====================================================
        // ACTIVITY POST ACTIONS (HR / USER)
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPost(string content, string? category)
        {
            if (!string.IsNullOrWhiteSpace(content))
            {
                var author = User.Identity?.Name ?? "HR / Team Member";
                var hrId = await GetEffectiveHrUserIdAsync();
                var post = new EngagementPost
                {
                    AuthorName = author,
                    Content = content.Trim(),
                    Category = string.IsNullOrWhiteSpace(category) ? "Shoutout" : category.Trim(),
                    CreatedAt = DateTime.Now,
                    HrUserId = (hrId.HasValue && hrId.Value > 0) ? hrId.Value : (CurrentUserId > 0 ? CurrentUserId : 1)
                };
                _context.EngagementPosts.Add(post);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Activity post published!";
            }
            return RedirectToAction(nameof(Engage));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int id)
        {
            var item = await _context.EngagementPosts.FindAsync(id);
            if (item != null)
            {
                _context.EngagementPosts.Remove(item);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Activity post deleted.";
            }
            return RedirectToAction(nameof(Engage));
        }

        [HttpPost]
        public async Task<IActionResult> LikePost(int id)
        {
            var post = await _context.EngagementPosts.FindAsync(id);
            if (post != null)
            {
                post.LikesCount += 1;
                await _context.SaveChangesAsync();
                return Json(new { success = true, likes = post.LikesCount });
            }
            return Json(new { success = false });
        }

        // =====================================================
        // ONBOARDING & OFFBOARDING (HR CUSTOMERS)
        // =====================================================
        [Route("Customers/OnboardingOffboarding")]
        [Route("Footer/OnboardingOffboarding")]
        [Route("OnboardingOffboarding")]
        public async Task<IActionResult> OnboardingOffboarding()
        {
            ViewBag.Title = "Employee Onboarding & Offboarding";
            ViewBag.Subtitle = "Seamless Employee Lifecycle Management: New Hire Provisioning & Exit Clearance";
            ViewBag.Description = "Manage new joiner onboarding checklists, document verification, asset handover, department clearances, and full-and-final exit settlements.";
            ViewBag.Theme = "purple";
            ViewBag.PageType = "onboardingoffboarding";

            var hrId = await GetEffectiveHrUserIdAsync();

            var onbQuery = _context.OnboardingRecords.AsQueryable();
            var offQuery = _context.OffboardingRecords.AsQueryable();
            var empQuery = _context.Employees.Where(e => e.IsActive);

            if (hrId.HasValue && hrId.Value > 0)
            {
                onbQuery = onbQuery.Where(o => o.HrUserId == hrId.Value);
                offQuery = offQuery.Where(o => o.HrUserId == hrId.Value);
                empQuery = empQuery.Where(e => e.HrUserId == hrId.Value);
            }

            var onboardings = await onbQuery
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var offboardings = await offQuery
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var activeEmployees = await empQuery
                .OrderBy(e => e.FirstName)
                .ToListAsync();

            ViewBag.Onboardings = onboardings;
            ViewBag.Offboardings = offboardings;
            ViewBag.ActiveEmployees = activeEmployees;

            ViewBag.ActiveOnboardingsCount = onboardings.Count(o => o.Status != "Completed");
            ViewBag.ActiveOffboardingsCount = offboardings.Count(o => o.Status != "Completed");
            ViewBag.CompletedOnboardingsCount = onboardings.Count(o => o.Status == "Completed");
            ViewBag.CompletedOffboardingsCount = offboardings.Count(o => o.Status == "Completed");

            return View("OnboardingOffboarding");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOnboarding(OnboardingRecord record)
        {
            if (!string.IsNullOrWhiteSpace(record.CandidateName) && !string.IsNullOrWhiteSpace(record.Email))
            {
                var hrId = await GetEffectiveHrUserIdAsync();
                record.CreatedAt = DateTime.Now;
                record.HrUserId = (hrId.HasValue && hrId.Value > 0) ? hrId.Value : (CurrentUserId > 0 ? CurrentUserId : 1);
                if (string.IsNullOrWhiteSpace(record.Status)) record.Status = "Pending";
                if (string.IsNullOrWhiteSpace(record.Department)) record.Department = "General";
                if (string.IsNullOrWhiteSpace(record.Designation)) record.Designation = "Team Member";
                record.RecalculateProgress();
                _context.OnboardingRecords.Add(record);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Onboarding initiated for {record.CandidateName}!";
            }
            return RedirectToAction(nameof(OnboardingOffboarding));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleOnboardingTask(int id, string taskKey)
        {
            var record = await _context.OnboardingRecords.FindAsync(id);
            if (record == null) return Json(new { success = false, message = "Record not found" });

            switch (taskKey?.ToLower())
            {
                case "docs":
                    record.HasDocumentsSubmitted = !record.HasDocumentsSubmitted;
                    break;
                case "asset":
                    record.HasAssetAssigned = !record.HasAssetAssigned;
                    break;
                case "email":
                    record.HasEmailCreated = !record.HasEmailCreated;
                    break;
                case "orientation":
                    record.HasOrientationDone = !record.HasOrientationDone;
                    break;
                case "bank":
                    record.HasBankDetailsVerified = !record.HasBankDetailsVerified;
                    break;
            }

            record.RecalculateProgress();
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                progress = record.ProgressPercentage,
                status = record.Status,
                docs = record.HasDocumentsSubmitted,
                asset = record.HasAssetAssigned,
                email = record.HasEmailCreated,
                orientation = record.HasOrientationDone,
                bank = record.HasBankDetailsVerified
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteOnboarding(int id, bool createEmployeeRecord = true)
        {
            var record = await _context.OnboardingRecords.FindAsync(id);
            if (record != null)
            {
                record.HasDocumentsSubmitted = true;
                record.HasAssetAssigned = true;
                record.HasEmailCreated = true;
                record.HasOrientationDone = true;
                record.HasBankDetailsVerified = true;
                record.ProgressPercentage = 100;
                record.Status = "Completed";

                if (createEmployeeRecord)
                {
                    bool exists = await _context.Employees.AnyAsync(e => e.Email.ToLower() == record.Email.ToLower());
                    if (!exists)
                    {
                        var parts = record.CandidateName.Trim().Split(' ', 2);
                        var firstName = parts[0];
                        var lastName = parts.Length > 1 ? parts[1] : "-";
                        var empCode = "EMP" + DateTime.Now.ToString("yyMM") + new Random().Next(100, 999);
                        var hrId = await GetEffectiveHrUserIdAsync();

                        var newEmp = new Employee
                        {
                            EmployeeCode = empCode,
                            FirstName = firstName,
                            LastName = lastName,
                            Email = record.Email.Trim(),
                            Phone = record.Phone?.Trim(),
                            Department = string.IsNullOrWhiteSpace(record.Department) ? "General" : record.Department.Trim(),
                            Designation = string.IsNullOrWhiteSpace(record.Designation) ? "Team Member" : record.Designation.Trim(),
                            JoiningDate = record.JoiningDate,
                            IsActive = true,
                            HrUserId = (hrId.HasValue && hrId.Value > 0) ? hrId.Value : (CurrentUserId > 0 ? CurrentUserId : 1)
                        };
                        _context.Employees.Add(newEmp);
                    }
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Onboarding marked completed for {record.CandidateName}! Workforce status updated.";
            }
            return RedirectToAction(nameof(OnboardingOffboarding));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOnboarding(int id)
        {
            var record = await _context.OnboardingRecords.FindAsync(id);
            if (record != null)
            {
                _context.OnboardingRecords.Remove(record);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Onboarding record deleted successfully.";
            }
            return RedirectToAction(nameof(OnboardingOffboarding));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOffboarding(OffboardingRecord record)
        {
            if (record.EmployeeId.HasValue && record.EmployeeId.Value > 0)
            {
                var emp = await _context.Employees.FindAsync(record.EmployeeId.Value);
                if (emp != null)
                {
                    record.EmployeeName = emp.FullName;
                    record.Department = emp.Department;
                    record.Designation = emp.Designation;
                }
            }

            if (!string.IsNullOrWhiteSpace(record.EmployeeName))
            {
                var hrId = await GetEffectiveHrUserIdAsync();
                record.CreatedAt = DateTime.Now;
                record.HrUserId = (hrId.HasValue && hrId.Value > 0) ? hrId.Value : (CurrentUserId > 0 ? CurrentUserId : 1);
                if (string.IsNullOrWhiteSpace(record.Status)) record.Status = "Initiated";
                if (string.IsNullOrWhiteSpace(record.ExitType)) record.ExitType = "Resignation";
                if (string.IsNullOrWhiteSpace(record.Department)) record.Department = "General";
                if (string.IsNullOrWhiteSpace(record.Designation)) record.Designation = "Team Member";
                record.RecalculateProgress();
                _context.OffboardingRecords.Add(record);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Offboarding exit process initiated for {record.EmployeeName}!";
            }
            return RedirectToAction(nameof(OnboardingOffboarding));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleOffboardingClearance(int id, string clearanceKey)
        {
            var record = await _context.OffboardingRecords.FindAsync(id);
            if (record == null) return Json(new { success = false, message = "Record not found" });

            switch (clearanceKey?.ToLower())
            {
                case "handover":
                    record.IsManagerHandoverDone = !record.IsManagerHandoverDone;
                    break;
                case "it":
                    record.IsItCleared = !record.IsItCleared;
                    break;
                case "finance":
                    record.IsFinanceCleared = !record.IsFinanceCleared;
                    break;
                case "exitinterview":
                    record.IsExitInterviewDone = !record.IsExitInterviewDone;
                    break;
                case "fnf":
                    record.IsFnfCompleted = !record.IsFnfCompleted;
                    break;
            }

            record.RecalculateProgress();
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                progress = record.ProgressPercentage,
                status = record.Status,
                handover = record.IsManagerHandoverDone,
                it = record.IsItCleared,
                finance = record.IsFinanceCleared,
                exitInterview = record.IsExitInterviewDone,
                fnf = record.IsFnfCompleted
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteOffboarding(int id, bool deactivateEmployee = true)
        {
            var record = await _context.OffboardingRecords.FindAsync(id);
            if (record != null)
            {
                record.IsManagerHandoverDone = true;
                record.IsItCleared = true;
                record.IsFinanceCleared = true;
                record.IsExitInterviewDone = true;
                record.IsFnfCompleted = true;
                record.ProgressPercentage = 100;
                record.Status = "Completed";

                if (deactivateEmployee && record.EmployeeId.HasValue)
                {
                    var emp = await _context.Employees.FindAsync(record.EmployeeId.Value);
                    if (emp != null)
                    {
                        emp.IsActive = false;
                    }
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Exit process finalized for {record.EmployeeName}! Clearances recorded.";
            }
            return RedirectToAction(nameof(OnboardingOffboarding));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOffboarding(int id)
        {
            var record = await _context.OffboardingRecords.FindAsync(id);
            if (record != null)
            {
                _context.OffboardingRecords.Remove(record);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Offboarding record deleted successfully.";
            }
            return RedirectToAction(nameof(OnboardingOffboarding));
        }

        public IActionResult About()
        {
            ViewBag.Title = "About Us";
            ViewBag.Subtitle = "Empowering Workplaces Through Intelligent HR Solutions";
            ViewBag.Description = "HR Attendance is a modern, all-in-one workforce management platform designed to automate attendance tracking, streamline leave operations, and simplify employee management.";
            ViewBag.Theme = "purple";
            ViewBag.PageType = "about";
            return View("Page");
        }


        public IActionResult Customers()
        {
            return Page(
                "Customers",
                "Trusted by Growing Organizations",
                "Discover how organizations use HR Attendance to simplify their workforce management.",
                     "orange"
            );
        }


        public IActionResult Partners()
        {
            return Page(
                "Partners",
                "Grow With HR Attendance",
                "Partner with HR Attendance to provide modern HR management solutions to organizations.",
                        "dark"
            );
        }


        public IActionResult Careers()
        {
            return Page(
                "Careers",
                "Build Your Career With Us",
                "Explore opportunities to work on modern HR technology and build solutions for businesses.",
                "purple"
            );
        }

        public IActionResult DocumentsAndPolicies()
        {
            ViewBag.Title = "Documents & Company Policies";
            ViewBag.Subtitle = "Official HR Governance, Compliance & Workplace Guidelines";
            ViewBag.Description = "Access essential employee documents, workplace codes of conduct, leave & attendance policies, data privacy terms, and standard operating procedures.";
            ViewBag.Theme = "purple";
            ViewBag.PageType = "policies";
            return View("Page");
        }
        public IActionResult Pricing()
        {
            ViewBag.Title = "Pricing";
            ViewBag.Subtitle = "Simple and Flexible Pricing";
            ViewBag.Description =
                "Choose the right HR Attendance plan for your organization. Start managing employees, attendance and HR operations with a plan that fits your business.";

            ViewBag.PageType = "pricing";
            ViewBag.Theme = "purple";

            return View("Page");
        }

        // =====================================================
        // CONTACT US
        // =====================================================

        public IActionResult ContactUs()
        {
            ViewBag.Title = "Contact Us";

            ViewBag.Subtitle = "We're here to help";

            ViewBag.Description =
                "Have a question about HR Attendance Management? Our team is ready to help you with your HR, attendance and employee management needs.";

            ViewBag.PageType = "contact";

            ViewBag.Theme = "purple";

            return View("Page");
        }


        // =====================================================
        // CONTACT US - SUBMIT
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitContact(ContactMessage model)
        {
            // Keep same page information
            ViewBag.Title = "Contact Us";

            ViewBag.Subtitle = "We're here to help";

            ViewBag.Description =
                "Have a question about HR Attendance Management? Our team is ready to help you with your HR, attendance and employee management needs.";

            ViewBag.PageType = "contact";

            ViewBag.Theme = "purple";


            // =================================================
            // VALIDATION
            // =================================================

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError(
                    nameof(model.Name),
                    "Please enter your name.");
            }


            if (string.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "Please enter your email.");
            }


            if (string.IsNullOrWhiteSpace(model.Subject))
            {
                ModelState.AddModelError(
                    nameof(model.Subject),
                    "Please enter subject.");
            }


            if (string.IsNullOrWhiteSpace(model.Message))
            {
                ModelState.AddModelError(
                    nameof(model.Message),
                    "Please enter your message.");
            }


            // =================================================
            // VALIDATION ERROR
            // =================================================

            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage =
                    "Please fill in all required fields correctly.";

                return View("Page", model);
            }


            // =================================================
            // SAVE TO DATABASE
            // =================================================

            try
            {
                await _contactService.CreateAsync(model);


                TempData["SuccessMessage"] =
                    "Thank you! Your message has been received. Our team will contact you shortly.";


                // Redirect prevents duplicate form submission
                return RedirectToAction(nameof(ContactUs));
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] =
                    "Something went wrong while saving your message. Please try again.";

                return RedirectToAction(nameof(ContactUs));
            }
        }
    }
}