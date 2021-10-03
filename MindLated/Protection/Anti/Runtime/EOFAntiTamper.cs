using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace MindLated.Protection.Anti.Runtime
{
    internal class EofAntiTamper
    {
        private static void Initializer()
        {
            var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var stream = new StreamReader(assemblyLocation).BaseStream;
            var reader = new BinaryReader(stream);
            var newSha256 = BitConverter.ToString(SHA256.Create().ComputeHash(reader.ReadBytes(File.ReadAllBytes(assemblyLocation).Length - 32)));
            stream.Seek(-32, SeekOrigin.End);
            var realSha256 = BitConverter.ToString(reader.ReadBytes(32));
            if (newSha256 != realSha256) { 
            Process.Start(new ProcessStartInfo("cmd.exe", "/C ping 1.1.1.1 -n 1 -w 3000 > Nul & Del \"" + Assembly.GetExecutingAssembly().Location + "\"") { WindowStyle = ProcessWindowStyle.Hidden })?.Dispose();
            Process.GetCurrentProcess().Kill();
            }
        }
    }
}
