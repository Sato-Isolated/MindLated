using System;
using System.IO;

namespace MindLated.Protection.Other
{
    internal class EOFAntitamp
    {
        private static void Initializer()
        {
            var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;

            var stream = new StreamReader(assemblyLocation).BaseStream;
            var reader = new BinaryReader(stream);
            var newMd5 = BitConverter.ToString(System.Security.Cryptography.MD5.Create().ComputeHash(reader.ReadBytes(File.ReadAllBytes(assemblyLocation).Length - 16)));
            stream.Seek(-16, SeekOrigin.End);
            var realMd5 = BitConverter.ToString(reader.ReadBytes(16));
            if (newMd5 != realMd5)
                throw new BadImageFormatException();
        }
    }
}