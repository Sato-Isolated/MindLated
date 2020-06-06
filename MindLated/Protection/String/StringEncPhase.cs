using dnlib.DotNet;
using dnlib.DotNet.Emit;
using MindLated.Services;
using System;
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
        private const string VIKey = "IzTdhG6S8uwg141S";

        public static void InjectClass(ModuleDef module)
        {
            var typeModule = ModuleDefMD.Load(typeof(EncryptionHelper).Module);
            var typeDef = typeModule.ResolveTypeDef(MDToken.ToRID(typeof(EncryptionHelper).MetadataToken));
            var members = InjectHelper.Inject(typeDef, module.GlobalType, module);
            MainWindow.init = (MethodDef)members.Single(method => method.Name == "Decrypt");
            foreach (var md in module.GlobalType.Methods)
            {
                if (md.Name != ".ctor") continue;
                module.GlobalType.Remove(md);
                break;
            }
        }

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
                        if (instr[i].OpCode != OpCodes.Ldstr) continue;
                        var originalSTR = instr[i].Operand as string;
                        var encodedSTR = Encrypt(originalSTR);
                        instr[i].Operand = encodedSTR;
                        instr.Insert(i + 1, Instruction.Create(OpCodes.Call, MainWindow.init));
                    }
                    method.Body.SimplifyBranches();
                }
            }
        }

        public static string Encrypt(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            var keyBytes = new Rfc2898DeriveBytes(PasswordHash, Encoding.ASCII.GetBytes(SaltKey)).GetBytes(256 / 8);
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