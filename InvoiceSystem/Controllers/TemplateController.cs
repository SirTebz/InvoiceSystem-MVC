using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InvoiceSystem.Data;
using InvoiceSystem.ViewModels;
using InvoiceSystem.Models;
using Microsoft.AspNetCore.Authorization;

namespace InvoiceSystem.Controllers
{
    public class TemplateController : Controller
    {
        private readonly ApplicationDbContext _context;
        public TemplateController(ApplicationDbContext context)
        {
            _context = context;
        }
        // GET: Template/Index
        // Retrieves and displays a list of all invoice templates.
        public async Task<IActionResult> Index()
        {
            var templates = await _context.Templates.AsNoTracking().ToListAsync();
            return View(templates);
        }
        // GET: Template/Create
        // Displays a form to create a new invoice template.
        public IActionResult Create()
        {
            return View(new InvoiceTemplateViewModel());
        }
        // POST: Template/Create
        // Processes the form submission for creating a new invoice template.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InvoiceTemplateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var template = new Template
                {
                    TemplateName = model.TemplateName,
                    HtmlContent = model.HtmlContent,
                    TemplateType = model.TemplateType,
                    CreatedDate = DateTime.Now,
                    CreatedBy = User.Identity?.Name ?? "Admin"
                };
                _context.Templates.Add(template);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }
        // GET: Template/Edit/{id}
        // Retrieves an existing invoice template for editing.
        public async Task<IActionResult> Edit(int id)
        {
            var template = await _context.Templates
            .AsNoTracking().FirstOrDefaultAsync(temp => temp.id == id);
            if (template == null)
            {
                return NotFound();
            }
            var viewModel = new InvoiceTemplateViewModel
            {
                Id = template.id,
                TemplateName = template.TemplateName,
                HtmlContent = template.HtmlContent,
                TemplateType = template.TemplateType
            };
            return View(viewModel);
        }
        // POST: Template/Edit/{id}
        // Processes the form submission for editing an invoice template.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(InvoiceTemplateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var template = await _context.Templates.FindAsync(model.Id);
                if (template == null)
                {
                    return NotFound();
                }
                template.TemplateName = model.TemplateName;
                template.HtmlContent = model.HtmlContent;
                template.TemplateType = model.TemplateType;
                template.UpdateDate = DateTime.Now;
                template.UpdateBy = User.Identity?.Name ?? "Admin";
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }
        // GET: Template/Delete/{id}
        // Displays a confirmation page to delete an invoice template.
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var template = await _context.Templates.AsNoTracking()
            .FirstOrDefaultAsync(t => t.id == id);
            if (template == null)
            {
                return NotFound();
            }
            return View(template);
        }
        // POST: Template/Delete/{id}
        // Processes the deletion of an invoice template.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var template = await _context.Templates.FindAsync(id);
            if (template == null)
            {
                return NotFound();
            }
            _context.Templates.Remove(template);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
