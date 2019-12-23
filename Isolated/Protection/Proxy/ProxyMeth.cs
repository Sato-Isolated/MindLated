using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Isolated.Protection.Proxy
{
    public static class ProxyMeth
    {
        public static Random rand = new Random();
        public static List<MemberRef> MemberRefList = new List<MemberRef>();

        //Scan de toutes les MemberRef
        public static void ScanMemberRef(ModuleDef module)
        {
            foreach (TypeDef type in module.Types)
            {
                foreach (MethodDef method in type.Methods)
                {
                    if (method.HasBody && method.Body.HasInstructions)
                    {
                        for (int i = 0; i < method.Body.Instructions.Count - 1; i++)
                        {
                            if (method.Body.Instructions[i].OpCode == OpCodes.Call)
                            {
                                try
                                {
                                    MemberRef original = (MemberRef)method.Body.Instructions[i].Operand;
                                    if (!original.HasThis)
                                    {
                                        MemberRefList.Add(original);
                                    }
                                }
                                catch
                                {
                                }
                            }
                        }
                    }
                }
            }
        }

        public static MethodDef GenerateSwitch(MemberRef original, ModuleDef md)
        {
            try
            {
                List<TypeSig> type = new List<TypeSig>();
                foreach (TypeSig sig in original.MethodSig.Params)
                {
                    type.Add(sig);
                }
                type.Add(md.CorLibTypes.Int32);
                MethodImplAttributes methImplFlags = MethodImplAttributes.IL | MethodImplAttributes.Managed;
                MethodAttributes methFlags = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot;
                MethodDef meth = new MethodDefUser("ProxyMeth" + rand.Next(0, int.MaxValue), MethodSig.CreateStatic(original.MethodSig.RetType, type.ToArray()), methImplFlags, methFlags);
                meth.Body = new CilBody();
                meth.Body.Variables.Add(new Local(md.CorLibTypes.Int32));
                meth.Body.Variables.Add(new Local(md.CorLibTypes.Int32));
                meth.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                List<Instruction> lst = new List<Instruction>();
                var switchs = new Instruction(OpCodes.Switch);
                meth.Body.Instructions.Add(switchs);
                var br_s = new Instruction(OpCodes.Br_S);
                meth.Body.Instructions.Add(br_s);
                for (int i = 0; i < 5; i++)
                {
                    for (int ia = 0; ia <= original.MethodSig.Params.Count - 1; ia++)
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
                br_s.Operand = ldnull;
                switchs.Operand = lst;
                return meth;
            }
            catch
            {
                return null;
            }
        }

        public static IEnumerable<T> Randomize<T>(IEnumerable<T> source)
        {
            Random rnd = new Random();
            return source.OrderBy<T, int>((item) => rnd.Next());
        }

        public static void Execute(ModuleDef module)
        {
            ScanMemberRef(module);
            foreach (TypeDef type in module.GetTypes())
            {
                if (type.IsGlobalModuleType) continue;
                foreach (MethodDef method in type.Methods.ToArray())
                {
                    if (!method.HasBody || method.Name.Contains("Proxy")) continue;
                    var instr = method.Body.Instructions;
                    for (int i = 0; i < instr.Count; i++)
                    {
                        if (method.Body.Instructions[i].OpCode == OpCodes.Call)
                        {
                            try
                            {
                                MemberRef original = (MemberRef)method.Body.Instructions[i].Operand;
                                if (!original.HasThis)
                                {
                                    MethodDef proxy = GenerateSwitch(original, module);
                                    method.DeclaringType.Methods.Add(proxy);
                                    instr[i].OpCode = OpCodes.Call;
                                    instr[i].Operand = proxy;
                                    int random = rand.Next(0, 5);
                                    for (int b = 0; b < proxy.Body.Instructions.Count - 1; b++)
                                    {
                                        if (proxy.Body.Instructions[b].OpCode == OpCodes.Ldc_I4)
                                        {
                                            if (proxy.Body.Instructions[b].Operand.ToString() == random.ToString())
                                            {
                                                proxy.Body.Instructions[b].OpCode = OpCodes.Call;
                                                proxy.Body.Instructions[b].Operand = original;
                                            }
                                            else
                                            {
                                                proxy.Body.Instructions[b].OpCode = OpCodes.Call;
                                                proxy.Body.Instructions[b].Operand = MemberRefList.Where(m => m.MethodSig.Params.Count == original.MethodSig.Params.Count).ToList().Random();
                                            }
                                        }
                                    }

                                    method.Body.Instructions.Insert(i, Instruction.CreateLdcI4(random));

                                    /*        MethodSig originalsignature = original.MethodSig;
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
                            }
                        }
                    }
                }
            }
        }
    }

    public static class EnumerableHelper<E>
    {
        private static Random r;

        static EnumerableHelper()
        {
            r = new Random();
        }

        public static T Random<T>(IEnumerable<T> input)
        {
            return input.ElementAt(r.Next(input.Count()));
        }
    }

    public static class EnumerableExtensions
    {
        public static T Random<T>(this IEnumerable<T> input)
        {
            return EnumerableHelper<T>.Random(input);
        }
    }
}