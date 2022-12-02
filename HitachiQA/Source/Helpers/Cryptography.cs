using System.Text;
using System.Security.Cryptography;
using System;
using System.IO;

namespace HitachiQA.Helpers
{
    public sealed class Cryptography
    {
        public static string Decrypt(string cipherText)
        {
            string EncryptionKey = "UMKY8RQAWB09791";
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes dec = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x31, 0x76, 0x81, 0x6b, 0x18, 0x4a, 0x67, 0x41, 0x76, 0x60, 0x4, 0x68, 0x81 });
                encryptor.Key = dec.GetBytes(32);
                encryptor.IV = dec.GetBytes(16);
                encryptor.Mode = CipherMode.CBC;
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }

        public static string Encrypt(string clearText)
        {
            string EncryptionKey = "UMKY8RQAWB09791";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes enc = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x31, 0x76, 0x81, 0x6b, 0x18, 0x4a, 0x67, 0x41, 0x76, 0x60, 0x4, 0x68, 0x81 });
                encryptor.Key = enc.GetBytes(32);
                encryptor.IV = enc.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }
    }
}
