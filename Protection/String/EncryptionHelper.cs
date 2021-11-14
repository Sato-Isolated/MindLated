using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace MindLated.Protection.String
{
    internal class EncryptionHelper
    {
        private static List<string> _list = new();

        public static void Generate()
        {
            using var manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("%Replace");
            using var streamReader = new StreamReader(new MemoryStream(UnHush(Read(manifestResourceStream))));
            _list = streamReader.ReadToEnd().Split(new[]
            {
                Environment.NewLine
            }, StringSplitOptions.None).ToList();
        }

        public static string Search(int key)
        {
            return _list.ElementAt(key);
        }

        private static byte[] Read(Stream input)
        {
            using var memoryStream = new MemoryStream();
            input.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }

        private static byte[] UnHush(IReadOnlyList<byte> text)
        {
            var key = new Rfc2898DeriveBytes("%Key1", Encoding.ASCII.GetBytes("%Key2")).GetBytes(256 / 8);
            var xor = new byte[text.Count];
            for (var i = 0; i < text.Count; i++)
            {
                xor[i] = (byte)(text[i] ^ key[i % key.Length]);
            }

            return xor;
        }

        public static string Decrypt(string encryptedText)
        {
            if (Assembly.GetCallingAssembly().FullName != Assembly.GetExecutingAssembly().FullName)
            {
                var cipherTextBytes = Convert.FromBase64String(encryptedText);
                var keyBytes = new Rfc2898DeriveBytes("%Key1", Encoding.ASCII.GetBytes("%Key2"))
                    .GetBytes(256 / 8);
                var symmetricKey = new RijndaelManaged()
                { Mode = CipherMode.CBC, Padding = PaddingMode.PKCS7 };

                var decryptor = symmetricKey.CreateDecryptor(keyBytes, Encoding.ASCII.GetBytes("%Key3"));
                var memoryStream = new MemoryStream(cipherTextBytes);
                var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
                var plainTextBytes = new byte[cipherTextBytes.Length];

                var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                memoryStream.Close();
                cryptoStream.Close();
                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount)
                    .TrimEnd("\0".ToCharArray());
            }
            return "MindLated.png";
        }
    }
}