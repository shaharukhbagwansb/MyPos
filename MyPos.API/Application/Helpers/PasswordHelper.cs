using System.Security.Cryptography;
using System.Text;

namespace MyPos.API.Application.Helpers
{
    public static class PasswordHelper
    {
        private static readonly string Key = "MyPosSecretKey123"; // 16 characters

        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            byte[] keyBytes = Encoding.UTF8.GetBytes(Key.PadRight(16).Substring(0, 16));
            byte[] iv = new byte[16];

            using var aes = Aes.Create();
            aes.Key = keyBytes;
            aes.IV = iv;

            using var encryptor = aes.CreateEncryptor();
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            return Convert.ToBase64String(encryptedBytes);
        }

        public static string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return encryptedText;

            try
            {
                byte[] keyBytes = Encoding.UTF8.GetBytes(Key.PadRight(16).Substring(0, 16));
                byte[] iv = new byte[16];
                byte[] encryptedBytes = Convert.FromBase64String(encryptedText);

                using var aes = Aes.Create();
                aes.Key = keyBytes;
                aes.IV = iv;

                using var decryptor = aes.CreateDecryptor();
                byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

                return Encoding.UTF8.GetString(decryptedBytes);
            }
            catch
            {
                return encryptedText;
            }
        }
    }
}