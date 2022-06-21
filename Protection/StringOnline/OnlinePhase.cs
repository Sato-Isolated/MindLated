using dnlib.DotNet;
using dnlib.DotNet.Emit;
using MindLated.Services;
using System;
using System.Linq;

namespace MindLated.Protection.StringOnline
{
    public static class OnlinePhase
    {
        public static void Execute(ModuleDef module)
        {
            InjectClass1(module);
            foreach (var type in module.GetTypes())
            {
                if (type.IsGlobalModuleType) continue;
                foreach (var meth in type.Methods)
                {
                    if (!meth.HasBody || !meth.Body.HasInstructions) continue;
                    if (meth.Name.Contains("Decoder")) continue;
                    for (var i = 0; i < meth.Body.Instructions.Count; i++)
                    {
                        if (meth.Body.Instructions[i].OpCode != OpCodes.Ldstr) continue;
                        var plainText = meth.Body.Instructions[i].Operand.ToString();
                        var operand = ConvertStringToHex(plainText!);
                        meth.Body.Instructions[i].Operand = operand;
                        meth.Body.Instructions.Insert(i + 1, Instruction.Create(OpCodes.Call, Form1.Init));
                    }
                    meth.Body.SimplifyBranches();
                }
            }
        }

        private static string ConvertStringToHex(string asciiString)
        {
            var hex = string.Empty;
            foreach (var c in asciiString)
            {
                int tmp = c;
                hex += $"{Convert.ToUInt32(tmp.ToString()):x2}";
            }
            return hex;
        }

        private static void InjectClass1(ModuleDef module)
        {
            var typeModule = ModuleDefMD.Load(typeof(OnlineString).Module);
            var typeDef = typeModule.ResolveTypeDef(MDToken.ToRID(typeof(OnlineString).MetadataToken));
            var members = InjectHelper.Inject(typeDef, module.GlobalType, module);
            Form1.Init = (MethodDef)members.Single(method => method.Name == "Decoder");
            foreach (var md in module.GlobalType.Methods)
            {
                if (md.Name != ".ctor") continue;
                module.GlobalType.Remove(md);
                break;
            }
        }
    }
}