using dnlib.DotNet;
using dnlib.DotNet.Emit;
using MindLated.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MindLated.Protection.String
{
    public static class StringEncPhase
    {
        private const string PasswordHash = "p7K95451qB88sZ7J";
        private const string SaltKey = "2GM23j301t60Z96T";
        private const string ViKey = "IzTdhG6S8uwg141S";

        private static void InjectClass(ModuleDef module)
        {
            var typeModule = ModuleDefMD.Load(typeof(EncryptionHelper).Module);
            var typeDef = typeModule.ResolveTypeDef(MDToken.ToRID(typeof(EncryptionHelper).MetadataToken));
            var members = InjectHelper.Inject(typeDef, module.GlobalType, module);
            var dnlibDefs = members as IDnlibDef[] ?? members.ToArray();
            MainWindow.Init = (MethodDef)dnlibDefs.Single(method => method.Name == "Decrypt");
            var cctor = module.GlobalType.FindStaticConstructor();
            MainWindow.Init2 = (MethodDef)dnlibDefs.Single(method => method.Name == "Search");
            var init = (MethodDef)dnlibDefs.Single(method => method.Name == "Generate");
            cctor.Body.Instructions.Insert(cctor.Body.Instructions.Count - 1, Instruction.Create(OpCodes.Call, init));
            foreach (var md in module.GlobalType.Methods)
            {
                if (md.Name == ".ctor")
                {
                    module.GlobalType.Remove(md);
                    break;
                }
            }
        }

        private static readonly List<string> Str = new();

        public static void Execute(ModuleDef module)
        {
            InjectClass(module);
            foreach (var type in module.GetTypes())
            {
                if (type.IsGlobalModuleType) continue;
                foreach (var method in type.Methods)
                {
                    if (!method.HasBody) continue;
                    var instr = method.Body.Instructions;
                    for (var i = 0; i < instr.Count; i++)
                    {
                        if (instr[i].OpCode == OpCodes.Ldstr)
                        {
                            var originalStr = instr[i].Operand as string;
                            var encodedStr = Encrypt(originalStr);
                            instr[i].Operand = encodedStr;
                            Str.Add(encodedStr);
                            instr.Insert(i + 1, Instruction.Create(OpCodes.Ldc_I4, Str.LastIndexOf(encodedStr)));
                            instr.Insert(i + 2, Instruction.Create(OpCodes.Call, MainWindow.Init2));
                            instr.Insert(i + 3, Instruction.Create(OpCodes.Call, MainWindow.Init));
                            instr.RemoveAt(i);
                        }
                    }
                    method.Body.SimplifyBranches();
                }
            }
            File.WriteAllLines($"{Path.GetTempPath()}List.txt", Str);
            var bytes = File.ReadAllBytes($"{Path.GetTempPath()}List.txt");
            module.Resources.Add(new EmbeddedResource("MindLated.zero", Hush(bytes), ManifestResourceAttributes.Public));
            File.Delete($"{Path.GetTempPath()}List.txt");
        }

        private static byte[] Hush(byte[] text)
        {
            var key = new Rfc2898DeriveBytes(PasswordHash, Encoding.ASCII.GetBytes(SaltKey)).GetBytes(256 / 8);
            var xor = new byte[text.Length];
            for (var i = 0; i < text.Length; i++)
            {
                xor[i] = (byte)(text[i] ^ key[i % key.Length]);
            }
            return xor;
        }

        private static string Encrypt(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            var keyBytes = new Rfc2898DeriveBytes(PasswordHash, Encoding.ASCII.GetBytes(SaltKey)).GetBytes(256 / 8);
            var symmetricKey = new RijndaelManaged { Mode = CipherMode.CBC, Padding = PaddingMode.PKCS7 };
            var encryptor = symmetricKey.CreateEncryptor(keyBytes, Encoding.ASCII.GetBytes(ViKey));
            byte[] cipherTextBytes;
            using var memoryStream = new MemoryStream();
            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
            {
                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                cryptoStream.FlushFinalBlock();
                cipherTextBytes = memoryStream.ToArray();
                cryptoStream.Close();
            }
            memoryStream.Close();
            return Convert.ToBase64String(cipherTextBytes);
        }
    }
}