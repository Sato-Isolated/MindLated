using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Isolated.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Isolated.Protection.Other
{
    public static class Anti_Tamper
    {
        public static void Md5(string filePath)
        {
            byte[] md5bytes = MD5.Create().ComputeHash(File.ReadAllBytes(filePath));
            using (var stream = new FileStream(filePath, FileMode.Append))
            {
                stream.Write(md5bytes, 0, md5bytes.Length);
            }
        }

        public static void Execute(ModuleDef module)
        {
            ModuleDefMD typeModule = ModuleDefMD.Load(typeof(EOFAntitamp).Module);
            MethodDef cctor = module.GlobalType.FindOrCreateStaticConstructor();
            TypeDef typeDef = typeModule.ResolveTypeDef(MDToken.ToRID(typeof(EOFAntitamp).MetadataToken));
            IEnumerable<IDnlibDef> members = InjectHelper.Inject(typeDef, module.GlobalType, module);
            var init = (MethodDef)members.Single(method => method.Name == "Initializer");
            cctor.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Call, init));
            foreach (MethodDef md in module.GlobalType.Methods)
            {
                if (md.Name == ".ctor")
                {
                    module.GlobalType.Remove(md);
                    break;
                }
            }
        }
    }
}