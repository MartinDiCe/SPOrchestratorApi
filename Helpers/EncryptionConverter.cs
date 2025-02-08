using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace SPOrchestratorAPI.Helpers
{
    public class EncryptionConverter : ValueConverter<string, string>
    {
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("1234567890123456"); 
        private static readonly byte[] IV = Encoding.UTF8.GetBytes("6543210987654321"); 

        public EncryptionConverter(ConverterMappingHints mappingHints = null)
            : base(
                  v => Encrypt(v),
                  v => Decrypt(v),
                  mappingHints)
        { }

        private static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;
            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new System.IO.MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                using var sw = new System.IO.StreamWriter(cs);
                sw.Write(plainText);
            }
            return Convert.ToBase64String(ms.ToArray());
        }

        private static string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return encryptedText;

            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;
            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            var buffer = Convert.FromBase64String(encryptedText);
            using var ms = new System.IO.MemoryStream(buffer);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new System.IO.StreamReader(cs);
            return sr.ReadToEnd();
        }
    }
}
