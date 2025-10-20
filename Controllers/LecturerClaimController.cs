using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using Contract_Monthly_Claim_System_Part2.Models;



namespace Contract_Monthly_Claim_System_Part2.Controllers
{
    public class LecturerClaimController : Controller
    {
        private readonly CMCSContext _context;

        public LecturerClaimController(CMCSContext context)
        {
            _context = context;
        }

        // GET: Create Claim
        public IActionResult Create()
        {
            return View();
        }

        // POST: Create Claim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Claim claim, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                // Handle file upload
                if (file != null)
                {
                    var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx" };
                    var extension = Path.GetExtension(file.FileName).ToLower();
                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("File", "Invalid file type. Only PDF, DOCX, XLSX allowed.");
                        return View(claim);
                    }

                    if (file.Length > 5 * 1024 * 1024) // 5MB limit
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

        // GET: List Lecturer Claims
        public IActionResult Index()
        {
            var claims = _context.Claims.ToList();
            return View(claims);
        }

        // Encryption helper
        private byte[] EncryptFile(byte[] data)
        {
            using Aes aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes("ThisIsASecretKey!"); // 16 chars
            aes.IV = Encoding.UTF8.GetBytes("ThisIsAnIV123456");   // 16 chars

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                cs.Write(data, 0, data.Length);
            }
            return ms.ToArray();
        }

        // Decrypt helper (for viewing files)
        private byte[] DecryptFile(byte[] encryptedData)
        {
            using Aes aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes("ThisIsASecretKey!");
            aes.IV = Encoding.UTF8.GetBytes("ThisIsAnIV123456");

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
            {
                cs.Write(encryptedData, 0, encryptedData.Length);
            }
            return ms.ToArray();
        }

        // Download file
        public IActionResult Download(int id)
        {
            var doc = _context.SupportingDocuments.Find(id);
            if (doc == null) return NotFound();

            var decrypted = DecryptFile(doc.EncryptedFile);
            return File(decrypted, "application/octet-stream", doc.FileName);
        }
    }
}