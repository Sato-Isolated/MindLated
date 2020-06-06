using dnlib.DotNet;
using dnlib.DotNet.Emit;
using MindLated.Protection.Renamer;
using System.Collections.Generic;
using System.Linq;

namespace MindLated.Protection.LocalF
{
    internal class L2F
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

        public static void Process(ModuleDef module, MethodDef method)
        {
            var instructions = method.Body.Instructions;
            foreach (var t in instructions)
            {
                if (!(t.Operand is Local local)) continue;
                FieldDef def;
                if (!convertedLocals.ContainsKey(local))
                {
                    def = new FieldDefUser(RenamerPhase.GenerateString(), new FieldSig(local.Type), FieldAttributes.Public | FieldAttributes.Static);
                    module.GlobalType.Fields.Add(def);
                    convertedLocals.Add(local, def);
                }
                else
                    def = convertedLocals[local];

                OpCode eq = null;
                switch (t.OpCode.Code)
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
                t.OpCode = eq;
                t.Operand = def;
            }
        }
    }
}