using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace HitachiQA.Helpers
{
    public sealed class Cryptography
    {
        private const string EncryptionKey = "UMKY8RQAWB09791";
        private static readonly byte[] Salt = new byte[] { 0x31, 0x76, 0x81, 0x6b, 0x18, 0x4a, 0x67, 0x41, 0x76, 0x60, 0x04, 0x68, 0x81 };
        private const int IterationCount = 100000;

        public static string Decrypt(string cipherText)
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                using (var dec = new Rfc2898DeriveBytes(EncryptionKey, Salt, IterationCount, HashAlgorithmName.SHA256))
                {
                    encryptor.Key = dec.GetBytes(32);
                    encryptor.IV = dec.GetBytes(16);
                }
                encryptor.Mode = CipherMode.CBC;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                    }
                    return Encoding.Unicode.GetString(ms.ToArray());
                }
            }
        }

        public static string Encrypt(string clearText)
        {
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                using (var enc = new Rfc2898DeriveBytes(EncryptionKey, Salt, IterationCount, HashAlgorithmName.SHA256))
                {
                    encryptor.Key = enc.GetBytes(32);
                    encryptor.IV = enc.GetBytes(16);
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }
    }
}
