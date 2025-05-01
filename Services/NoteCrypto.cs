using System.Security.Cryptography;
using System.Text;

namespace USBVault.Services
{
    public static class NoteCrypto
    {
        public static byte[] Encrypt(string data, byte[] key)
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.GenerateIV();

            using var ms = new MemoryStream();
            ms.Write(aes.IV, 0, 16);
            using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(Encoding.UTF8.GetBytes(data));
            }
            return ms.ToArray();
        }

        public static string Decrypt(byte[] encrypted, byte[] key)
        {
            using var aes = Aes.Create();
            aes.Key = key;

            var iv = new byte[16];
            Array.Copy(encrypted, 0, iv, 0, 16);
            aes.IV = iv;

            using var ms = new MemoryStream(encrypted, 16, encrypted.Length - 16);
            using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }
    }
}