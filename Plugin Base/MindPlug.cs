using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;

namespace Plugin_Base
{
    public class MindPlug
    {
        public static void Execute(ModuleDefMD mod)
        {
            foreach (var type in mod.GetTypes())
            {
                if (type.IsGlobalModuleType) continue;
                foreach (var methodDef2 in type.Methods)
                {
                    if (!methodDef2.HasBody) continue;
                    var instr = methodDef2.Body.Instructions;
                    for (var i = 0; i < instr.Count; i++)
                    {
                        if (!methodDef2.Body.Instructions[i].IsLdcI4()) continue;
                        var rnd = new Random();
                        var randomuint = rnd.Next(2147483647);
                        methodDef2.Body.Instructions.Insert(i + 1, Instruction.Create(OpCodes.Sizeof, methodDef2.Module.Import(typeof(bool))));
                        methodDef2.Body.Instructions.Insert(i + 2, Instruction.Create(OpCodes.Add));
                        methodDef2.Body.Instructions.Insert(i + 3, Instruction.Create(OpCodes.Ldc_R8, Math.PI / 2));
                        methodDef2.Body.Instructions.Insert(i + 4, Instruction.Create(OpCodes.Call, methodDef2.Module.Import(typeof(Math).GetMethod("Sin", new[] { typeof(double) }))));
                        methodDef2.Body.Instructions.Insert(i + 5, Instruction.Create(OpCodes.Conv_I4));
                        methodDef2.Body.Instructions.Insert(i + 6, Instruction.Create(OpCodes.Sub));
                        methodDef2.Body.Instructions.Insert(i + 7, Instruction.Create(OpCodes.Sizeof, methodDef2.Module.Import(typeof(bool))));
                        methodDef2.Body.Instructions.Insert(i + 8, Instruction.Create(OpCodes.Add));
                        methodDef2.Body.Instructions.Insert(i + 9, Instruction.Create(OpCodes.Ldc_R8, Math.PI / randomuint));
                        methodDef2.Body.Instructions.Insert(i + 10, Instruction.Create(OpCodes.Call, methodDef2.Module.Import(typeof(Math).GetMethod("Cos", new[] { typeof(double) }))));
                        methodDef2.Body.Instructions.Insert(i + 11, Instruction.Create(OpCodes.Conv_I4));
                        methodDef2.Body.Instructions.Insert(i + 12, Instruction.Create(OpCodes.Sub));
                    }
                }
            }
        }
    }
}