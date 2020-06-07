using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;

namespace MindLated.Protection.Proxy
{
    internal class ProxyString
    {
        public static Random rand = new Random();

        public static void Execute(ModuleDef module)
        {
            foreach (var type in module.GetTypes())
            {
                if (type.IsGlobalModuleType) continue;
                foreach (var method in type.Methods)
                {
                    if (!method.HasBody) continue;
                    var instr = method.Body.Instructions;
                    foreach (var t in instr)
                    {
                        if (t.OpCode != OpCodes.Ldstr) continue;
                        var methImplFlags = MethodImplAttributes.IL | MethodImplAttributes.Managed;
                        var methFlags = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot;
                        var meth1 = new MethodDefUser("ProxyMeth" + rand.Next(0, int.MaxValue),
                            MethodSig.CreateStatic(module.CorLibTypes.String),
                            methImplFlags, methFlags);
                        module.GlobalType.Methods.Add(meth1);
                        meth1.Body = new CilBody();
                        meth1.Body.Variables.Add(new Local(module.CorLibTypes.String));
                        meth1.Body.Instructions.Add(Instruction.Create(OpCodes.Ldstr, t.Operand.ToString()));
                        meth1.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

                        t.OpCode = OpCodes.Call;
                        t.Operand = meth1;
                    }
                }
            }
        }
    }
}