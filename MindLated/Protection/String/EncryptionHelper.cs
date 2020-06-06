using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace MindLated.Protection.String
{
    internal class EncryptionHelper
    {
        private const string PasswordHash = "p7K95451qB88sZ7J";
        private const string SaltKey = "2GM23j301t60Z96T";
        private const string VIKey = "IzTdhG6S8uwg141S";

        public static string Decrypt(string encryptedText)
        {
            if (Assembly.GetExecutingAssembly() != Assembly.GetCallingAssembly()) return "Isolated.png";
            var cipherTextBytes = Convert.FromBase64String(encryptedText);
            var keyBytes = new Rfc2898DeriveBytes(PasswordHash, Encoding.ASCII.GetBytes(SaltKey)).GetBytes(256 / 8);
            var symmetricKey = new RijndaelManaged() { Mode = CipherMode.CBC, Padding = PaddingMode.PKCS7 };

            var decryptor = symmetricKey.CreateDecryptor(keyBytes, Encoding.ASCII.GetBytes(VIKey));
            var memoryStream = new MemoryStream(cipherTextBytes);
            var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            var plainTextBytes = new byte[cipherTextBytes.Length];

            var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
            memoryStream.Close();
            cryptoStream.Close();
            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount).TrimEnd("\0".ToCharArray());
        }
    }
}