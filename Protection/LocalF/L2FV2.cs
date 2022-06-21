using dnlib.DotNet;
using dnlib.DotNet.Emit;
using MindLated.Protection.Renamer;
using System.Collections.Generic;
using System.Linq;

namespace MindLated.Protection.LocalF
{
    internal class L2FV2
    {
        private static Dictionary<Local, FieldDef> _convertedLocals = new();

        public static void Execute(ModuleDef module)
        {
            foreach (var type in module.Types.Where(x => x != module.GlobalType))
            {
                foreach (var meth in type.Methods.Where(x => x.HasBody && x.Body.HasInstructions && !x.IsConstructor))
                {
                    _convertedLocals = new Dictionary<Local, FieldDef>();
                    Process(module, meth);
                }
            }
        }

        private static void Process(ModuleDef module, MethodDef meth)
        {
            meth.Body.SimplifyMacros(meth.Parameters);
            var instructions = meth.Body.Instructions;
            foreach (var t in instructions)
            {
                if (t.Operand is not Local local) continue;
                FieldDef def;
                if (!_convertedLocals.ContainsKey(local))
                {
                    def = new FieldDefUser(RenamerPhase.GenerateString(RenamerPhase.RenameMode.Normal), new FieldSig(local.Type), FieldAttributes.Public | FieldAttributes.Static);
                    module.GlobalType.Fields.Add(def);
                    _convertedLocals.Add(local, def);
                }
                else
                    def = _convertedLocals[local];

                var eq = t.OpCode?.Code switch
                {
                    Code.Ldloc => OpCodes.Ldsfld,
                    Code.Ldloca => OpCodes.Ldsflda,
                    Code.Stloc => OpCodes.Stsfld,
                    _ => null
                };
                t.OpCode = eq;
                t.Operand = def;
            }
            _convertedLocals.ToList().ForEach(x => meth.Body.Variables.Remove(x.Key));
            _convertedLocals = new Dictionary<Local, FieldDef>();
        }
    }
}