using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Isolated.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Isolated.Protection.String
{
    public static class StringEncPhase
    {
        private static readonly string PasswordHash = "p7K95451qB88sZ7J";
        private static readonly string SaltKey = "2GM23j301t60Z96T";
        private static readonly string VIKey = "IzTdhG6S8uwg141S";

        public static void InjectClass(ModuleDef module)
        {
            ModuleDefMD typeModule = ModuleDefMD.Load(typeof(EncryptionHelper).Module);
            TypeDef typeDef = typeModule.ResolveTypeDef(MDToken.ToRID(typeof(EncryptionHelper).MetadataToken));
            IEnumerable<IDnlibDef> members = InjectHelper.Inject(typeDef, module.GlobalType, module);
            Form1.init = (MethodDef)members.Single(method => method.Name == "Decrypt");
            foreach (MethodDef md in module.GlobalType.Methods)
            {
                if (md.Name == ".ctor")
                {
                    module.GlobalType.Remove(md);
                    break;
                }
            }
        }

        public static void Execute(ModuleDef module)
        {
            InjectClass(module);
            foreach (TypeDef type in module.GetTypes())
            {
                if (type.IsGlobalModuleType) continue;
                foreach (MethodDef method in type.Methods)
                {
                    if (!method.HasBody) continue;
                    var instr = method.Body.Instructions;
                    for (int i = 0; i < instr.Count; i++)
                    {
                        if (instr[i].OpCode == OpCodes.Ldstr)
                        {
                            var originalSTR = instr[i].Operand as string;
                            var encodedSTR = Encrypt(originalSTR);
                            instr[i].Operand = encodedSTR;
                            instr.Insert(i + 1, Instruction.Create(OpCodes.Call, Form1.init));
                        }
                    }
                    method.Body.SimplifyBranches();
                }
            }
        }

        public static string Encrypt(string plainText)
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] keyBytes = new Rfc2898DeriveBytes(PasswordHash, Encoding.ASCII.GetBytes(SaltKey)).GetBytes(256 / 8);
            var symmetricKey = new RijndaelManaged() { Mode = CipherMode.CBC, Padding = PaddingMode.PKCS7 };
            var encryptor = symmetricKey.CreateEncryptor(keyBytes, Encoding.ASCII.GetBytes(VIKey));
            byte[] cipherTextBytes;
            using (var memoryStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                    cryptoStream.FlushFinalBlock();
                    cipherTextBytes = memoryStream.ToArray();
                    cryptoStream.Close();
                }
                memoryStream.Close();
            }
            return Convert.ToBase64String(cipherTextBytes);
        }
    }
}