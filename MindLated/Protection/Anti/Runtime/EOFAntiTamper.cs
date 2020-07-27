using System;
using System.IO;
using System.Security.Cryptography;

namespace MindLated.Protection.Anti.Runtime
{
    internal class EOFAntitamp
    {
        private static void Initializer()
        {
            var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;

            var stream = new StreamReader(assemblyLocation).BaseStream;
            var reader = new BinaryReader(stream);
            stream.Seek(-32, SeekOrigin.End);
            var realSha256 = BitConverter.ToString(reader.ReadBytes(32));
            if (string.Compare(BitConverter.ToString(SHA256.Create().ComputeHash(reader.ReadBytes(File.ReadAllBytes(assemblyLocation).Length - 32))), realSha256, StringComparison.Ordinal) != 0)
                throw new BadImageFormatException();
        }
    }
}