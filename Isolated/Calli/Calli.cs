using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Linq;

namespace Isolated.Protection.Calli
{
    internal class Calli
    {
        private static int Amount { get; set; }

        public static void Execute(ModuleDef module)
        {
            foreach (TypeDef type in module.Types.ToArray())
            {
                foreach (MethodDef method in type.Methods.ToArray())
                {
                    if (method.HasBody)
                    {
                        if (method.Body.HasInstructions)
                        {
                            if (method.FullName.Contains("My.")) continue;
                            if (method.FullName.Contains(".My")) continue;
                            if (method.IsConstructor) continue;
                            if (method.DeclaringType.IsGlobalModuleType) continue;
                            for (int i = 0; i < method.Body.Instructions.Count - 1; i++)
                            {
                                try
                                {
                                    if (method.Body.Instructions[i].ToString().Contains("ISupportInitialize")) continue;
                                    if (method.Body.Instructions[i].OpCode == OpCodes.Call || method.Body.Instructions[i].OpCode == OpCodes.Callvirt || method.Body.Instructions[i].OpCode == OpCodes.Ldloc_S)
                                    {
                                        try
                                        {
                                            MemberRef membertocalli = (MemberRef)method.Body.Instructions[i].Operand;
                                            method.Body.Instructions[i].OpCode = OpCodes.Calli;
                                            method.Body.Instructions[i].Operand = membertocalli.MethodSig;
                                            method.Body.Instructions.Insert(i, Instruction.Create(OpCodes.Ldftn, membertocalli));
                                            ++Amount;
                                        }
                                        catch (Exception ex)
                                        {
                                            string str = ex.Message;
                                        }
                                    }
                                }
                                catch { }
                            }
                        }
                        else { }
                    }
                }
                foreach (MethodDef md in module.GlobalType.Methods)
                {
                    if (md.Name == ".ctor")
                    {
                        module.GlobalType.Remove(md);
                        break;
                    }
                }
            }
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"   Calli called {Amount}.");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
