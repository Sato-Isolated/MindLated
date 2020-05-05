using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Isolated.Protection.Renamer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Isolated.Protection.LocalF
{
    internal class L2F
    {
        private static Dictionary<Local, FieldDef> convertedLocals = new Dictionary<Local, FieldDef>();
        private static int Amount { get; set; }

        public static void Execute(ModuleDef Module)
        {
            foreach (var type in Module.Types.Where(x => x != Module.GlobalType))
            {
                foreach (var method2 in type.Methods.Where(x => x.HasBody && x.Body.HasInstructions && !x.IsConstructor))
                {
                    convertedLocals = new Dictionary<Local, FieldDef>();
                    Process(Module, method2);
                }
            }
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"    L2F Converted {Amount}.");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void Process(ModuleDef Module, MethodDef method)
        {
            var instructions = method.Body.Instructions;
            for (int i = 0; i < instructions.Count; i++)
            {
                if (instructions[i].Operand is Local local)
                {
                    FieldDef def = null;
                    if (!convertedLocals.ContainsKey(local))
                    {
                        def = new FieldDefUser(RenamerPhase.GenerateString(), new FieldSig(local.Type), FieldAttributes.Public | FieldAttributes.Static);
                        Module.GlobalType.Fields.Add(def);
                        convertedLocals.Add(local, def);
                    }
                    else
                        def = convertedLocals[local];

                    OpCode eq = null;
                    switch (instructions[i].OpCode.Code)
                    {
                        case Code.Ldloc:
                        case Code.Ldloc_S:
                        case Code.Ldloc_0:
                        case Code.Ldloc_1:
                        case Code.Ldloc_2:
                        case Code.Ldloc_3:
                            eq = OpCodes.Ldsfld;
                            break;

                        case Code.Ldloca:
                        case Code.Ldloca_S:
                            eq = OpCodes.Ldsflda;
                            break;

                        case Code.Stloc:
                        case Code.Stloc_0:
                        case Code.Stloc_1:
                        case Code.Stloc_2:
                        case Code.Stloc_3:
                        case Code.Stloc_S:
                            eq = OpCodes.Stsfld;
                            break;
                    }
                    instructions[i].OpCode = eq;
                    instructions[i].Operand = def;
                    ++Amount;
                }
            }
        }
    }
}