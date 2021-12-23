using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MindLated.Protection.Proxy
{
    public static class ProxyMeth
    {
        private static readonly Random Rand = new();
        private static readonly List<MemberRef> MemberRefList = new();

        //Scan de toutes les MemberRef
        private static void ScanMemberRef(ModuleDef module)
        {
            foreach (var type in module.Types)
            {
                foreach (var method in type.Methods)
                {
                    if (!method.HasBody || !method.Body.HasInstructions) continue;
                    for (var i = 0; i < method.Body.Instructions.Count - 1; i++)
                    {
                        if (method.Body.Instructions[i].OpCode != OpCodes.Call) continue;
                        try
                        {
                            var original = (MemberRef)method.Body.Instructions[i].Operand;
                            if (!original.HasThis)
                            {
                                MemberRefList.Add(original);
                            }
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }
            }
        }

        private static MethodDef GenerateSwitch(MemberRef original, ModuleDef md)
        {
            try
            {
                var type = original.MethodSig.Params.ToList();
                type.Add(md.CorLibTypes.Int32);
                var methImplFlags = MethodImplAttributes.IL | MethodImplAttributes.Managed;
                var methFlags = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot;
                MethodDef meth = new MethodDefUser($"ProxyMeth{Rand.Next(0, int.MaxValue)}", MethodSig.CreateStatic(original.MethodSig.RetType, type.ToArray()), methImplFlags, methFlags)
                {
                    Body = new CilBody()
                };
                meth.Body.Variables.Add(new Local(md.CorLibTypes.Int32));
                meth.Body.Variables.Add(new Local(md.CorLibTypes.Int32));
                meth.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                var lst = new List<Instruction>();
                var switchs = new Instruction(OpCodes.Switch);
                meth.Body.Instructions.Add(switchs);
                var brS = new Instruction(OpCodes.Br_S);
                meth.Body.Instructions.Add(brS);
                for (var i = 0; i < 5; i++)
                {
                    for (var ia = 0; ia <= original.MethodSig.Params.Count - 1; ia++)
                    {
                        meth.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg, meth.Parameters[ia]));
                        if (ia == 0)
                        {
                            lst.Add(Instruction.Create(OpCodes.Ldarg, meth.Parameters[ia]));
                        }
                    }
                    var ldstr = Instruction.Create(OpCodes.Ldc_I4, i);
                    meth.Body.Instructions.Add(ldstr);
                    meth.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                }

                var ldnull = Instruction.Create(OpCodes.Ldnull);
                meth.Body.Instructions.Add(ldnull);
                meth.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                brS.Operand = ldnull;
                switchs.Operand = lst;
                return meth;
            }
            catch
            {
                return null!;
            }
        }

        public static void Execute(ModuleDef module)
        {
            ScanMemberRef(module);
            foreach (var type in module.GetTypes())
            {
                if (type.IsGlobalModuleType) continue;
                foreach (var method in type.Methods.ToArray())
                {
                    if (!method.HasBody || method.Name.Contains("ProxyMeth")) continue;
                    var instr = method.Body.Instructions;
                    for (var i = 0; i < instr.Count; i++)
                    {
                        if (method.Body.Instructions[i].OpCode != OpCodes.Call) continue;
                        try
                        {
                            var original = (MemberRef)method.Body.Instructions[i].Operand;
                            if (!original.HasThis)
                            {
                                var proxy = GenerateSwitch(original, module);
                                method.DeclaringType.Methods.Add(proxy);
                                instr[i].OpCode = OpCodes.Call;
                                instr[i].Operand = proxy;
                                var random = Rand.Next(0, 5);
                                for (var b = 0; b < proxy.Body.Instructions.Count - 1; b++)
                                {
                                    if (proxy.Body.Instructions[b].OpCode == OpCodes.Ldc_I4)
                                    {
                                        if (string.Compare(proxy.Body.Instructions[b].Operand.ToString(), random.ToString(), StringComparison.Ordinal) != 0)
                                        {
                                            proxy.Body.Instructions[b].OpCode = OpCodes.Call;
                                            proxy.Body.Instructions[b].Operand = MemberRefList.Where(m => m.MethodSig.Params.Count == original.MethodSig.Params.Count).ToList().Random();
                                        }
                                        else
                                        {
                                            proxy.Body.Instructions[b].OpCode = OpCodes.Call;
                                            proxy.Body.Instructions[b].Operand = original;
                                        }
                                    }
                                }

                                method.Body.Instructions.Insert(i, Instruction.CreateLdcI4(random));

                                /*   MethodSig originalsignature = original.MethodSig;
                                   var methImplFlags = MethodImplAttributes.IL | MethodImplAttributes.Managed;
                                   var methFlags = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot;
                                   var meth1 = new MethodDefUser("ProxyMeth" + rand.Next(0, int.MaxValue).ToString(),
                                              originalsignature,
                                               methImplFlags, methFlags);
                                   module.GlobalType.Methods.Add(meth1);
                                   meth1.Body = new CilBody();
                                   for (int ia = 0; ia <= originalsignature.Params.Count - 1; ia++)
                                   {
                                       meth1.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg, meth1.Parameters[ia]));
                                   }
                                   meth1.Body.Instructions.Add(Instruction.Create(OpCodes.Call, original));
                                   meth1.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                                   instr[i].OpCode = OpCodes.Call;
                                   instr[i].Operand = meth1;*/
                            }
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }
            }
        }
    }

    public static class EnumerableHelper
    {
        private static readonly Random R;

        static EnumerableHelper()
        {
            R = new Random();
        }

        public static TE Random<TE>(IEnumerable<TE> input)
        {
            var enumerable = input as TE[] ?? input.ToArray();
            return enumerable.ElementAt(R.Next(enumerable.Length));
        }
    }

    public static class EnumerableExtensions
    {
        public static T Random<T>(this IEnumerable<T> input)
        {
            return EnumerableHelper.Random(input);
        }
    }
}