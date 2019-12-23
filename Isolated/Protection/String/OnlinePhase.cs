using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Isolated.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Isolated.Protection.String
{
    public static class OnlinePhase
    {
        public static void Execute(ModuleDef module)
        {
            InjectClass1(module);
            foreach (TypeDef type in module.GetTypes())
            {
                if (type.IsGlobalModuleType) continue;
                foreach (MethodDef methodDef2 in type.Methods)
                {
                    bool hasBody = methodDef2.HasBody;
                    if (hasBody)
                    {
                        bool hasInstructions = methodDef2.Body.HasInstructions;
                        if (hasInstructions)
                        {
                            bool flag2 = !methodDef2.Name.Contains("Decoder");
                            if (flag2)
                            {
                                for (int i = 0; i < methodDef2.Body.Instructions.Count; i++)
                                {
                                    bool flag3 = methodDef2.Body.Instructions[i].OpCode == OpCodes.Ldstr;
                                    if (flag3)
                                    {
                                        string plainText = methodDef2.Body.Instructions[i].Operand.ToString();

                                        string operand = ConvertStringToHex(plainText);
                                        methodDef2.Body.Instructions[i].Operand = operand;
                                        Random rnd = new Random();
                                        int randomuint = rnd.Next(2147483647);
                                        methodDef2.Body.Instructions.Insert(i + 1, Instruction.Create(OpCodes.Call, Form1.init));
                                    }
                                }
                                methodDef2.Body.SimplifyBranches();
                            }
                        }
                    }
                }
            }
        }

        public static string ConvertStringToHex(string asciiString)
        {
            string hex = "";
            foreach (char c in asciiString)
            {
                int tmp = c;
                hex += string.Format("{0:x2}", Convert.ToUInt32(tmp.ToString()));
            }
            return hex;
        }

        public static void InjectClass1(ModuleDef module)
        {
            ModuleDefMD typeModule = ModuleDefMD.Load(typeof(OnlineString).Module);
            TypeDef typeDef = typeModule.ResolveTypeDef(MDToken.ToRID(typeof(OnlineString).MetadataToken));
            IEnumerable<IDnlibDef> members = InjectHelper.Inject(typeDef, module.GlobalType, module);
            Form1.init = (MethodDef)members.Single(method => method.Name == "Decoder");
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