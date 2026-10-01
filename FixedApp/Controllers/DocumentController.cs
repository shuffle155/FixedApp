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
        private readonly ApplicationDbContext context;
        private readonly IAuthorizationService authorizationService;
        private readonly IHashids hashids;

        public DocumentController(ApplicationDbContext context,
            IAuthorizationService authorizationService, IHashids hashids)
        {
            this.context = context;
            this.authorizationService = authorizationService;
            this.hashids = hashids;
        }

        private async Task<Document> GetDocumentAsync(string hashId)
        {
            var decodedId = hashids.Decode(hashId);
            if (decodedId.Length == 0)
            {
                return null;
            }
            int id = decodedId[0];
            var document = await context.Documents
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
            var documents = context.Documents.ToList();
            var userId = User.FindFirst(ClaimTypes.NameIdentifier);
            if (User.IsInRole("Admin"))
            {
                documents = context.Documents.ToList();
            }
            else
            {
                int uid = int.Parse(userId.Value);
                documents = context.Documents.Where(d => d.OwnerId == uid)
                    .ToList();
            }
            ViewBag.Hashids = hashids;
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
            var authRes = await authorizationService
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
            var authRes = await authorizationService
                .AuthorizeAsync(User, document, new IsOwnerRequirement());
            if (!authRes.Succeeded)
            {
                return Forbid();
            }
            ViewBag.HashId = hashids.Encode(document.Id);
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
            var authRes = await authorizationService
                .AuthorizeAsync(User, document, new IsOwnerRequirement());
            if (!authRes.Succeeded)
            {
                return Forbid();
            }
            document.Content = content;
            await context.SaveChangesAsync();
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
            var authRes = await authorizationService
                .AuthorizeAsync(User, document, new IsOwnerRequirement());
            if (!authRes.Succeeded)
            {
                return Forbid();
            }
            context.Documents.Remove(document);
            await context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}