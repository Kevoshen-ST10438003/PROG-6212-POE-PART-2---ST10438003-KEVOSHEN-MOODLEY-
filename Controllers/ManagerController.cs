using Contract_Monthly_Claim_System_Part2.Helpers;
using Contract_Monthly_Claim_System_Part2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Contract_Monthly_Claim_System_Part2.Controllers
{
    public class ManagerController : Controller
    {
        private readonly CMCSContext _context;

        public ManagerController(CMCSContext context)
        {
            _context = context;
        }


        public IActionResult Index()
        {
            var pendingClaims = _context.Claims
                .Include(c => c.Documents)
                .Where(c => c.Status == ClaimStatus.CoordinatorApproved)
                .ToList();

            return View(pendingClaims);
        }

       
        public IActionResult History()
        {
            var historyClaims = _context.Claims
                .Include(c => c.Documents)
                .Where(c => c.Status == ClaimStatus.ManagerApproved || c.Status == ClaimStatus.Rejected)
                .OrderByDescending(c => c.SubmissionDate)
                .ToList();

            return View(historyClaims);
        }


        public IActionResult Review(int id)
        {
            var claim = _context.Claims
                .Include(c => c.Documents)
                .FirstOrDefault(c => c.ClaimID == id);

            if (claim == null) return NotFound();
            return View(claim);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Review(int id, string decision)
        {
            var claim = _context.Claims.Find(id);
            if (claim == null) return NotFound();

            if (decision == "Approve")
            {
                claim.Status = ClaimStatus.ManagerApproved;
                TempData["Message"] = "✅ Claim fully approved.";
                TempData["AlertClass"] = "alert-success";
            }
            else if (decision == "Reject")
            {
                claim.Status = ClaimStatus.Rejected;
                TempData["Message"] = "❌ Claim rejected.";
                TempData["AlertClass"] = "alert-danger";
            }

            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }


        public IActionResult Download(int docId)
        {
            var document = _context.SupportingDocuments
                .FirstOrDefault(d => d.SupportingDocumentID == docId);

            if (document == null)
                return NotFound("Document not found.");

            
            var decryptedBytes = DecryptFile(document.EncryptedFile);

            return File(decryptedBytes, "application/octet-stream", document.FileName);
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
    }
}