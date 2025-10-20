using Contract_Monthly_Claim_System_Part2.Helpers;
using Contract_Monthly_Claim_System_Part2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;


namespace Contract_Monthly_Claim_System_Part2.Controllers
{
    public class CoordinatorController : Controller
    {
        private readonly CMCSContext _context;

        public CoordinatorController(CMCSContext context)
        {
            _context = context;
        }

       
        public IActionResult Index()
        {
          
            var pendingClaims = _context.Claims
                .Include(c => c.Documents)
                .Where(c => c.Status == "Pending")
                .ToList();
            return View(pendingClaims);
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
                claim.Status = "Coordinator Approved";
                TempData["Message"] = "✅ Claim verified successfully.";
                TempData["AlertClass"] = "alert-success";
            }
            else if (decision == "Reject")
            {
                claim.Status = "Rejected";
                TempData["Message"] = "❌ Claim rejected.";
                TempData["AlertClass"] = "alert-danger";
            }

            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Download(int docId)
        {
            var document = _context.SupportingDocuments.FirstOrDefault(d => d.SupportingDocumentID == docId);

            if (document == null)
            {
                return NotFound("Document not found.");
            }

            var encryptedPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", document.FileName);

            if (!System.IO.File.Exists(encryptedPath))
            {
                return NotFound("File not found on server.");
            }

            var decryptedBytes = FileEncryptionHelper.DecryptFile(encryptedPath);

            
            return File(decryptedBytes, "application/octet-stream", document.FileName);
        }

      
        private byte[] DecryptFile(byte[] encryptedData)
        {
            using var aes = System.Security.Cryptography.Aes.Create();
            aes.Key = System.Text.Encoding.UTF8.GetBytes("ThisIsASecretKey!");
            aes.IV = System.Text.Encoding.UTF8.GetBytes("ThisIsAnIV123456");

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, decryptor, System.Security.Cryptography.CryptoStreamMode.Write))
            {
                cs.Write(encryptedData, 0, encryptedData.Length);
            }
            return ms.ToArray();
        }
    }
}