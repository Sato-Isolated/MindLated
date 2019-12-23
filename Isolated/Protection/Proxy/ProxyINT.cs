using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;

namespace Isolated.Protection.Proxy
{
    public static class ProxyINT
    {
        public static Random rand = new Random();

        public static void Execute(ModuleDef module)
        {
            foreach (TypeDef type in module.GetTypes())
            {
                if (type.IsGlobalModuleType) continue;
                foreach (MethodDef method in type.Methods)
                {
                    if (!method.HasBody) continue;
                    var instr = method.Body.Instructions;
                    for (int i = 0; i < instr.Count; i++)
                    {
                        if (method.Body.Instructions[i].IsLdcI4())
                        {
                            var methImplFlags = MethodImplAttributes.IL | MethodImplAttributes.Managed;
                            var methFlags = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot;
                            var meth1 = new MethodDefUser("ProxyMeth" + rand.Next(0, int.MaxValue).ToString(),
                                        MethodSig.CreateStatic(module.CorLibTypes.Int32),
                                        methImplFlags, methFlags);
                            module.GlobalType.Methods.Add(meth1);
                            meth1.Body = new CilBody();
                            meth1.Body.Variables.Add(new Local(module.CorLibTypes.Int32));
                            meth1.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4, instr[i].GetLdcI4Value()));
                            meth1.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                            instr[i].OpCode = OpCodes.Call;
                            instr[i].Operand = meth1;
                        }
                        else if (method.Body.Instructions[i].OpCode == OpCodes.Ldc_R4)
                        {
                            var methImplFlags = MethodImplAttributes.IL | MethodImplAttributes.Managed;
                            var methFlags = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot;
                            var meth1 = new MethodDefUser("ProxyMeth" + rand.Next(0, int.MaxValue).ToString(),
                                        MethodSig.CreateStatic(module.CorLibTypes.Double),
                                        methImplFlags, methFlags);
                            module.GlobalType.Methods.Add(meth1);
                            meth1.Body = new CilBody();
                            meth1.Body.Variables.Add(new Local(module.CorLibTypes.Double));
                            meth1.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_R4, (float)method.Body.Instructions[i].Operand));
                            meth1.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                            instr[i].OpCode = OpCodes.Call;
                            instr[i].Operand = meth1;
                        }
                    }
                }
            }
        }
    }
}