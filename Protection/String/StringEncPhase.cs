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
        private static string key1 = Renamer.RenamerPhase.GenerateString(Renamer.RenamerPhase.RenameMode.Key);
        private static string key2 = Renamer.RenamerPhase.GenerateString(Renamer.RenamerPhase.RenameMode.Key);
        private static string key3 = Renamer.RenamerPhase.GenerateString(Renamer.RenamerPhase.RenameMode.Key);

        private static void InjectClass(ModuleDef module)
        {
            var typeModule = ModuleDefMD.Load(typeof(EncryptionHelper).Module);
            var typeDef = typeModule.ResolveTypeDef(MDToken.ToRID(typeof(EncryptionHelper).MetadataToken));
            var members = InjectHelper.Inject(typeDef, module.GlobalType, module);
            var dnlibDefs = members as IDnlibDef[] ?? members.ToArray();
            Form1.Init = (MethodDef)dnlibDefs.Single(method => method.Name == "Decrypt");
            var cctor = module.GlobalType.FindStaticConstructor();
            Form1.Init2 = (MethodDef)dnlibDefs.Single(method => method.Name == "Search");
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
            var p = Renamer.RenamerPhase.GenerateString(Renamer.RenamerPhase.RenameMode.Normal);
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
                            instr.Insert(i + 2, Instruction.Create(OpCodes.Call, Form1.Init2));
                            instr.Insert(i + 3, Instruction.Create(OpCodes.Call, Form1.Init));
                            instr.RemoveAt(i);
                        }
                    }
                    method.Body.SimplifyBranches();
                }
            }
            File.WriteAllLines($"{Path.GetTempPath()}List.txt", Str);
            var bytes = File.ReadAllBytes($"{Path.GetTempPath()}List.txt");
            module.Resources.Add(new EmbeddedResource(p, Hush(bytes), ManifestResourceAttributes.Public));
            foreach (var type in module.GetTypes())
            {
                foreach (var method in type.Methods)
                {
                    if (!method.HasBody) continue;

                    var instr = method.Body.Instructions;
                    for (var i = 0; i < instr.Count; i++)
                    {
                        if (instr[i].OpCode == OpCodes.Ldstr)
                        {
                            if (instr[i].Operand as string == "%Replace")
                            {
                                instr[i].Operand = p;
                            }
                            if (instr[i].Operand as string == "%Key1")
                            {
                                instr[i].Operand = key1;
                            }
                            if (instr[i].Operand as string == "%Key2")
                            {
                                instr[i].Operand = key2;
                            }
                            if (instr[i].Operand as string == "%Key3")
                            {
                                instr[i].Operand = key3;
                            }
                        }
                    }
                    method.Body.SimplifyBranches();
                }
            }
            File.Delete($"{Path.GetTempPath()}List.txt");
        }



        private static byte[] Hush(IReadOnlyList<byte> text)
        {
            var key = new Rfc2898DeriveBytes(key1, Encoding.ASCII.GetBytes(key2)).GetBytes(256 / 8);
            var xor = new byte[text.Count];
            for (var i = 0; i < text.Count; i++)
            {
                xor[i] = (byte)(text[i] ^ key[i % key.Length]);
            }
            return xor;
        }

        private static string Encrypt(string plainText)
        {

            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            var keyBytes = new Rfc2898DeriveBytes(key1, Encoding.ASCII.GetBytes(key2)).GetBytes(256 / 8);
            var symmetricKey = new RijndaelManaged { Mode = CipherMode.CBC, Padding = PaddingMode.PKCS7 };
            var encryptor = symmetricKey.CreateEncryptor(keyBytes, Encoding.ASCII.GetBytes(key3));
            using var memoryStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.FlushFinalBlock();
            var cipherTextBytes = memoryStream.ToArray();
            cryptoStream.Close();
            memoryStream.Close();
            return Convert.ToBase64String(cipherTextBytes);
        }
    }
}