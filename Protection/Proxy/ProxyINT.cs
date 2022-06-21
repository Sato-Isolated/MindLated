using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace MindLated.Protection.Proxy
{
    public static class ProxyInt
    {
        public static void Execute(ModuleDef module)
        {
            foreach (var type in module.GetTypes())
            {
                if (type.IsGlobalModuleType) continue;
                foreach (var meth in type.Methods)
                {
                    if (!meth.HasBody) continue;
                    var instr = meth.Body.Instructions;
                    for (var i = 0; i < instr.Count; i++)
                    {
                        if (meth.Body.Instructions[i].IsLdcI4())
                        {
                            var methImplFlags = MethodImplAttributes.IL | MethodImplAttributes.Managed;
                            var methFlags = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot;
                            var meth1 = new MethodDefUser(Renamer.RenamerPhase.GenerateString(Renamer.RenamerPhase.RenameMode.Normal),
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
                        else if (meth.Body.Instructions[i].OpCode == OpCodes.Ldc_R4)
                        {
                            var methImplFlags = MethodImplAttributes.IL | MethodImplAttributes.Managed;
                            var methFlags = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot;
                            var meth1 = new MethodDefUser(Renamer.RenamerPhase.GenerateString(Renamer.RenamerPhase.RenameMode.Normal),
                                        MethodSig.CreateStatic(module.CorLibTypes.Double),
                                        methImplFlags, methFlags);
                            module.GlobalType.Methods.Add(meth1);
                            meth1.Body = new CilBody();
                            meth1.Body.Variables.Add(new Local(module.CorLibTypes.Double));
                            meth1.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_R4, (float)meth.Body.Instructions[i].Operand));
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