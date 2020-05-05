using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Isolator.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Isolator.Protection.LocalField
{
    internal class L2FV2 : Randomizer
    {
        private static int Amount { get; set; }
        private static Dictionary<Local, FieldDef> convertedLocals = new Dictionary<Local, FieldDef>();

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
            Console.WriteLine($"    L2FV2 Converted {Amount}.");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void Process(ModuleDef Module, MethodDef method)
        {
            method.Body.SimplifyMacros(method.Parameters);
            var instructions = method.Body.Instructions;
            for (int i = 0; i < instructions.Count; i++)
            {
                if (instructions[i].Operand is Local local)
                {
                    FieldDef def = null;
                    if (!convertedLocals.ContainsKey(local))
                    {
                        def = new FieldDefUser(GenerateRandomString(7), new FieldSig(local.Type), FieldAttributes.Public | FieldAttributes.Static);
                        Module.GlobalType.Fields.Add(def);
                        convertedLocals.Add(local, def);
                    }
                    else
                        def = convertedLocals[local];

                    OpCode eq = null;
                    switch (instructions[i].OpCode.Code)
                    {
                        case Code.Ldloc:
                            eq = OpCodes.Ldsfld;
                            break;

                        case Code.Ldloca:
                            eq = OpCodes.Ldsflda;
                            break;

                        case Code.Stloc:
                            eq = OpCodes.Stsfld;
                            break;
                    }
                    instructions[i].OpCode = eq;
                    instructions[i].Operand = def;
                    ++Amount;
                }
            }
            convertedLocals.ToList().ForEach(x => method.Body.Variables.Remove(x.Key));
            convertedLocals = new Dictionary<Local, FieldDef>();
        }
    }
}