using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;

namespace MindLated.Protection.INT
{
    public static class AddIntPhase
    {
        public static void Execute(ModuleDef module)
        {
            foreach (var type in module.GetTypes())
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
                        methodDef2.Body.Instructions.Insert(i + 4, Instruction.Create(OpCodes.Call, methodDef2.Module.Import(typeof(Math).GetMethod("Sin", new Type[] { typeof(double) }))));
                        methodDef2.Body.Instructions.Insert(i + 5, Instruction.Create(OpCodes.Conv_I4));
                        methodDef2.Body.Instructions.Insert(i + 6, Instruction.Create(OpCodes.Sub));
                        methodDef2.Body.Instructions.Insert(i + 7, Instruction.Create(OpCodes.Sizeof, methodDef2.Module.Import(typeof(bool))));
                        methodDef2.Body.Instructions.Insert(i + 8, Instruction.Create(OpCodes.Add));
                        methodDef2.Body.Instructions.Insert(i + 9, Instruction.Create(OpCodes.Ldc_R8, Math.PI / randomuint));
                        methodDef2.Body.Instructions.Insert(i + 10, Instruction.Create(OpCodes.Call, methodDef2.Module.Import(typeof(Math).GetMethod("Cos", new Type[] { typeof(double) }))));
                        methodDef2.Body.Instructions.Insert(i + 11, Instruction.Create(OpCodes.Conv_I4));
                        methodDef2.Body.Instructions.Insert(i + 12, Instruction.Create(OpCodes.Sub));
                    }
                }
            }
        }

        public static void Execute2(ModuleDef md)
        {
            foreach (var type in md.GetTypes())
            {
                if (type.IsGlobalModuleType) continue;
                foreach (var method in type.Methods)
                {
                    if (!method.HasBody) continue;
                    {
                        for (var i = 0; i < method.Body.Instructions.Count; i++)
                        {
                            if (!method.Body.Instructions[i].IsLdcI4()) continue;
                            var numorig = new Random(Guid.NewGuid().GetHashCode()).Next();
                            var div = new Random(Guid.NewGuid().GetHashCode()).Next();
                            var num = numorig ^ div;

                            var nop = OpCodes.Nop.ToInstruction();

                            var local = new Local(method.Module.ImportAsTypeSig(typeof(int)));
                            method.Body.Variables.Add(local);

                            method.Body.Instructions.Insert(i + 1, OpCodes.Stloc.ToInstruction(local));
                            method.Body.Instructions.Insert(i + 2, Instruction.Create(OpCodes.Ldc_I4, method.Body.Instructions[i].GetLdcI4Value() - sizeof(float)));
                            method.Body.Instructions.Insert(i + 3, Instruction.Create(OpCodes.Ldc_I4, num));
                            method.Body.Instructions.Insert(i + 4, Instruction.Create(OpCodes.Ldc_I4, div));
                            method.Body.Instructions.Insert(i + 5, Instruction.Create(OpCodes.Xor));
                            method.Body.Instructions.Insert(i + 6, Instruction.Create(OpCodes.Ldc_I4, numorig));
                            method.Body.Instructions.Insert(i + 7, Instruction.Create(OpCodes.Bne_Un, nop));
                            method.Body.Instructions.Insert(i + 8, Instruction.Create(OpCodes.Ldc_I4, 2));
                            method.Body.Instructions.Insert(i + 9, OpCodes.Stloc.ToInstruction(local));
                            method.Body.Instructions.Insert(i + 10, Instruction.Create(OpCodes.Sizeof, method.Module.Import(typeof(float))));
                            method.Body.Instructions.Insert(i + 11, Instruction.Create(OpCodes.Add));
                            method.Body.Instructions.Insert(i + 12, nop);
                            i += 12;
                        }
                        method.Body.SimplifyBranches();
                    }
                }
            }
        }
    }
}