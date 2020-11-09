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
                foreach (var method in type.Methods.ToArray())
                {
                    if (!method.HasBody) continue;
                    if (!method.Body.HasInstructions) continue;
                    if (method.FullName.Contains("My.")) continue;
                    if (method.FullName.Contains(".My")) continue;
                    if (method.FullName.Contains("Costura")) continue;
                    if (method.IsConstructor) continue;
                    if (method.DeclaringType.IsGlobalModuleType) continue;
                    for (var i = 0; i < method.Body.Instructions.Count - 1; i++)
                    {
                        try
                        {
                            if (method.Body.Instructions[i].ToString().Contains("ISupportInitialize") || (method.Body.Instructions[i].OpCode != OpCodes.Call &&
                                method.Body.Instructions[i].OpCode != OpCodes.Callvirt &&
                                method.Body.Instructions[i].OpCode != OpCodes.Ldloc_S)) continue;

                            if (method.Body.Instructions[i].ToString().Contains("Object") || (method.Body.Instructions[i].OpCode != OpCodes.Call &&
                                method.Body.Instructions[i].OpCode != OpCodes.Callvirt &&
                                method.Body.Instructions[i].OpCode != OpCodes.Ldloc_S)) continue;

                            try
                            {
                                var membertocalli = (MemberRef)method.Body.Instructions[i].Operand;
                                method.Body.Instructions[i].OpCode = OpCodes.Calli;
                                method.Body.Instructions[i].Operand = membertocalli.MethodSig;
                                method.Body.Instructions.Insert(i, Instruction.Create(OpCodes.Ldftn, membertocalli));
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