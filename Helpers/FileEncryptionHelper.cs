using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Contract_Monthly_Claim_System_Part2.Helpers
{
    public static class FileEncryptionHelper
    {
        private static readonly string encryptionKey = "CMCSSecretKey2025"; 

        public static void EncryptAndSaveFile(Stream inputStream, string outputPath)
        {
            using (Aes aes = Aes.Create())
            {
                var key = new Rfc2898DeriveBytes(encryptionKey, Encoding.UTF8.GetBytes("CMCS2025Salt"));
                aes.Key = key.GetBytes(32);
                aes.IV = key.GetBytes(16);

                using (var cryptoStream = new CryptoStream(File.Create(outputPath), aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    inputStream.CopyTo(cryptoStream);
                }
            }
        }

        
        public static byte[] DecryptFile(string encryptedFilePath)
        {
            using (Aes aes = Aes.Create())
            {
                var key = new Rfc2898DeriveBytes(encryptionKey, Encoding.UTF8.GetBytes("CMCS2025Salt"));
                aes.Key = key.GetBytes(32);
                aes.IV = key.GetBytes(16);

                using (var inputStream = File.OpenRead(encryptedFilePath))
                using (var cryptoStream = new CryptoStream(inputStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
                using (var memoryStream = new MemoryStream())
                {
                    cryptoStream.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
        }
    }
}
