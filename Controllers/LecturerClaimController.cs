using Contract_Monthly_Claim_System_Part2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Contract_Monthly_Claim_System_Part2.Controllers
{
    public class LecturerClaimController : Controller
    {
        private readonly CMCSContext _context;

        public LecturerClaimController(CMCSContext context)
        {
            _context = context;
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Claim claim, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                if (file != null)
                {
                    var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx" };
                    var extension = Path.GetExtension(file.FileName).ToLower();
                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("File", "Invalid file type. Only PDF, DOCX, XLSX allowed.");
                        return View(claim);
                    }

                    if (file.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("File", "File too large. Maximum size is 5MB.");
                        return View(claim);
                    }

                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    var encrypted = EncryptFile(ms.ToArray());

                    claim.Documents = new List<SupportingDocument>
                    {
                        new SupportingDocument
                        {
                            FileName = file.FileName,
                            EncryptedFile = encrypted
                        }
                    };
                }

                _context.Claims.Add(claim);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(claim);
        }

        public IActionResult Index()
        {
            var claims = _context.Claims.ToList();
            return View(claims);
        }

        private byte[] EncryptFile(byte[] data)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes("12345678901234567890123456789012");
            aes.IV = Encoding.UTF8.GetBytes("1234567890123456");

            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write)
            {
            };
            cs.Write(data, 0, data.Length);
            cs.FlushFinalBlock();
            return ms.ToArray();
        }

        private byte[] DecryptFile(byte[] encryptedData)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes("12345678901234567890123456789012");
            aes.IV = Encoding.UTF8.GetBytes("1234567890123456");

            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(encryptedData, 0, encryptedData.Length);
            cs.FlushFinalBlock();
            return ms.ToArray();
        }

        public IActionResult Download(int id)
        {
            var doc = _context.SupportingDocuments.Find(id);
            if (doc == null) return NotFound();

            var decrypted = DecryptFile(doc.EncryptedFile);
            return File(decrypted, "application/octet-stream", doc.FileName);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var claim = await _context.Claims
                .Include(c => c.Documents)
                .FirstOrDefaultAsync(c => c.ClaimID == id);

            if (claim == null) return NotFound();
            return View(claim);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var claim = await _context.Claims
                .Include(c => c.Documents)
                .FirstOrDefaultAsync(c => c.ClaimID == id);

            if (claim != null)
            {
                if (claim.Documents != null)
                {
                    _context.SupportingDocuments.RemoveRange(claim.Documents);
                }

                _context.Claims.Remove(claim);
                await _context.SaveChangesAsync();
                TempData["Message"] = "🗑️ Claim deleted successfully.";
                TempData["AlertClass"] = "alert-success";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
