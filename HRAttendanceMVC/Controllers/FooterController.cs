using HRAttendanceMVC.Application.Interfaces;
using HRAttendanceMVC.Domain.Enities;
using HRAttendanceMVC.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace HRAttendanceMVC.Controllers
{
    public class FooterController : Controller
    {
        // =====================================================
        // CONTACT SERVICE
        // =====================================================

        private readonly IContactService _contactService;


        public FooterController(IContactService contactService)
        {
            _contactService = contactService;
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
            return Page(
                "What is HRMS?",
                "Human Resource Management System",
                "HRMS is a centralized system that helps organizations manage employees, attendance, leave, payroll and HR processes.",
                 "purple"
            );
        }


        public IActionResult GuideLeaveManagement()
        {
            return Page(
                "Guide to Leave Management",
                "Manage Employee Leave Efficiently",
                "Learn how organizations can simplify leave policies, approvals, balances and leave reporting.",
                 "dark"
            );
        }


        public IActionResult GuideAttendanceManagement()
        {
            return Page(
                "Guide to Attendance Management",
                "Everything About Employee Attendance",
                "Learn how to monitor attendance, working hours, late arrivals, absences and employee schedules.",
                   "green"
            );
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
            return Page(
                "HR Help",
                "We're Here to Help",
                "Find helpful information and support for managing your HR Attendance system.",
                       "green"
            );
        }


        public IActionResult Videos()
        {
            return Page(
                "Videos",
                "Learn With HR Videos",
                "Watch useful videos and tutorials about HR management, attendance and employee management.",
                     "blue"
            );
        }


        // =========================
        // COMPANY
        // =========================

        public IActionResult About()
        {
            return Page(
                "About Us",
                "About HR Attendance",
                "HR Attendance helps organizations simplify employee management, attendance, leave and HR operations.",
                        "pink"
            );
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