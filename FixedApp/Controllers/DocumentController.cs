using FixedApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HashidsNet;
using FixedApp.Authorization;
using Microsoft.EntityFrameworkCore;

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

        private async Task<Document?> GetDocumentAsync(string hashId)
        {
            var decodedId = _hashids.Decode(hashId);
            if (decodedId.Length == 0)
            {
                return null;
            }
            int id = decodedId[0];
            var document = await _context.Documents
                .FirstOrDefaultAsync(d => d.Id == id);
            if (document == null)
            {
                return null;
            }
            return document;
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
            ViewBag.Hashids = _hashids;
            return View(documents);
        }

        [Authorize]
        public async Task<IActionResult> Details(string hashId)
        {
            Document document = await GetDocumentAsync(hashId);
            if (document == null)
            {
                return NotFound();
            }
            var authRes = await _authorizationService
                .AuthorizeAsync(User, document, new IsOwnerRequirement());
            if (!authRes.Succeeded)
            {
                return Forbid();
            }
            return View(document);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(string hashId)
        {
            Document document = await GetDocumentAsync(hashId);
            if (document == null)
            {
                return NotFound();
            }
            var authRes = await _authorizationService
                .AuthorizeAsync(User, document, new IsOwnerRequirement());
            if (!authRes.Succeeded)
            {
                return Forbid();
            }
            ViewBag.HashId = _hashids.Encode(document.Id);
            return View(document);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string hashId, string content)
        {
            Document document = await GetDocumentAsync(hashId);
            if (document == null)
            {
                return NotFound();
            }
            var authRes = await _authorizationService
                .AuthorizeAsync(User, document, new IsOwnerRequirement());
            if (!authRes.Succeeded)
            {
                return Forbid();
            }
            document.Content = content;
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string hashId)
        {
            Document document = await GetDocumentAsync(hashId);
            if (document == null)
            {
                return NotFound();
            }
            var authRes = await _authorizationService
                .AuthorizeAsync(User, document, new IsOwnerRequirement());
            if (!authRes.Succeeded)
            {
                return Forbid();
            }
            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
