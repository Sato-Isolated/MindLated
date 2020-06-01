using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Isolated.Protection.Renamer;
using System.Collections.Generic;
using System.Linq;

namespace Isolated.Protection.LocalF
{
    internal class L2FV2
    {
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
        }

        public static void Process(ModuleDef Module, MethodDef method)
        {
            method.Body.SimplifyMacros(method.Parameters);
            var instructions = method.Body.Instructions;
            foreach (var t in instructions)
            {
                if (!(t.Operand is Local local)) continue;
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
                switch (t.OpCode.Code)
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
                t.OpCode = eq;
                t.Operand = def;
            }
            convertedLocals.ToList().ForEach(x => method.Body.Variables.Remove(x.Key));
            convertedLocals = new Dictionary<Local, FieldDef>();
        }
    }
}