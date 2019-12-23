using System;
using System.IO;

namespace Isolated.Protection.Other
{
    internal class EOFAntitamp
    {
        private static void Initializer()
        {
            string assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;

            Stream stream = new StreamReader(assemblyLocation).BaseStream;
            BinaryReader reader = new BinaryReader(stream);
            string realMd5 = null, newMd5 = null;
            newMd5 = BitConverter.ToString(System.Security.Cryptography.MD5.Create().ComputeHash(reader.ReadBytes(System.IO.File.ReadAllBytes(assemblyLocation).Length - 16)));
            stream.Seek(-16, SeekOrigin.End);
            realMd5 = BitConverter.ToString(reader.ReadBytes(16));
            if (newMd5 != realMd5)
                throw new BadImageFormatException();
        }
    }
}