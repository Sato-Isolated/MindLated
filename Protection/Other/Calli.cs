using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Linq;

namespace MindLated.Protection.Other
{
    internal class Calli
    {
        public static void Execute(ModuleDef module)
        {
            foreach (var type in module.Types.ToArray())
            {
                foreach (var meth in type.Methods.ToArray())
                {
                    if (!meth.HasBody) continue;
                    if (!meth.Body.HasInstructions) continue;
                    if (meth.FullName.Contains("My.")) continue;
                    if (meth.FullName.Contains(".My")) continue;
                    if (meth.FullName.Contains("Costura")) continue;
                    if (meth.IsConstructor) continue;
                    if (meth.DeclaringType.IsGlobalModuleType) continue;
                    for (var i = 0; i < meth.Body.Instructions.Count - 1; i++)
                    {
                        try
                        {
                            if (meth.Body.Instructions[i].ToString().Contains("ISupportInitialize") || meth.Body.Instructions[i].OpCode != OpCodes.Call &&
                                meth.Body.Instructions[i].OpCode != OpCodes.Callvirt &&
                                meth.Body.Instructions[i].OpCode != OpCodes.Ldloc_S) continue;

                            if (meth.Body.Instructions[i].ToString().Contains("Object") || meth.Body.Instructions[i].OpCode != OpCodes.Call &&
                                meth.Body.Instructions[i].OpCode != OpCodes.Callvirt &&
                                meth.Body.Instructions[i].OpCode != OpCodes.Ldloc_S) continue;

                            try
                            {
                                var membertocalli = (MemberRef)meth.Body.Instructions[i].Operand;
                                meth.Body.Instructions[i].OpCode = OpCodes.Calli;
                                meth.Body.Instructions[i].Operand = membertocalli.MethodSig;
                                meth.Body.Instructions.Insert(i, Instruction.Create(OpCodes.Ldftn, membertocalli));
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                }
                foreach (var md in module.GlobalType.Methods)
                {
                    if (md.Name != ".ctor") continue;
                    module.GlobalType.Remove(md);
                    break;
                }
            }
        }
    }
}