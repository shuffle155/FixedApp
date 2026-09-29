using FixedApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HashidsNet;
using FixedApp.Authorization;

namespace FixedApp.Controllers
{
    public class DocumentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthorizationService _authorizationService;
        private readonly IHashids _hashids;

        public DocumentController(ApplicationDbContext context,
            IAuthorizationService authorizationService, IHashids hashids)
        {
            _context = context;
            _authorizationService = authorizationService;
            _hashids = hashids;
        }

        [Authorize]
        public IActionResult Index()
        {
            var documents = _context.Documents.ToList();
            var userId = User.FindFirst(ClaimTypes.NameIdentifier);
            if (User.IsInRole("Admin"))
            {
                documents = _context.Documents.ToList();
            }
            else
            {
                int uid = int.Parse(userId.Value);
                documents = _context.Documents.Where(d => d.OwnerId == uid)
                    .ToList();
            }
            return View(documents);
        }

        [Authorize]
        public IActionResult Details(int id)
        {
            var document = _context.Documents.FirstOrDefault(d => d.Id == id);
            if (document == null)
            {
                return NotFound();
            }
            return View(document);
        }

        [Authorize(Roles = "Admin, User")]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var document = _context.Documents.FirstOrDefault(d => d.Id == id);
            if (document == null)
            {
                return NotFound();
            }
            return View(document);
        }

        [Authorize(Roles = "Admin, User")]
        [HttpPost]
        public IActionResult Edit(int id, string content)
        {
            var document = _context.Documents.FirstOrDefault(d => d.Id == id);
            if (document == null)
            {
                return NotFound();
            }
            document.Content = content;
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var document = _context.Documents.FirstOrDefault(d => d.Id == id);
            if (document == null)
            {
                return NotFound();
            }
            _context.Documents.Remove(document);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
