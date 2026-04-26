using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Collections.Generic;
using System.Linq;

namespace MindLated.Protection.CtrlFlow
{
    public static class JumpCFlow
    {
        public static void Execute(ModuleDefMD module)
        {
            foreach (var type in module.Types)
            {
                foreach (var meth in type.Methods.ToArray())
                {
                    if (!meth.HasBody || !meth.Body.HasInstructions || meth.Body.HasExceptionHandlers) continue;
                    if (meth.IsConstructor || meth.IsGetter || meth.IsSetter || meth.Body.Instructions.Count < 4) continue;
                    if (ControlFlowObfuscation.WasTransformed(meth)) continue;
                    if (HasComplexControlFlow(meth.Body.Instructions)) continue;

                    if (!TryCollectZeroDepthEntries(meth.Body.Instructions, out var zeroDepthEntries)) continue;

                    var protectedTargets = CollectTargets(meth.Body.Instructions);
                    for (var i = 0; i < meth.Body.Instructions.Count - 1; i++)
                    {
                        var current = meth.Body.Instructions[i];
                        var inst = meth.Body.Instructions[i + 1];
                        if (protectedTargets.Contains(inst)) continue;
                        if (!zeroDepthEntries.Contains(inst)) continue;
                        if (current.OpCode.FlowControl == FlowControl.Branch ||
                            current.OpCode.FlowControl == FlowControl.Cond_Branch ||
                            current.OpCode.FlowControl == FlowControl.Return ||
                            current.OpCode.FlowControl == FlowControl.Throw)
                            continue;

                        meth.Body.Instructions.Insert(i + 1, Instruction.Create(OpCodes.Br, inst));
                        meth.Body.Instructions.Insert(i + 2, Instruction.Create(OpCodes.Ldstr, Renamer.RenamerPhase.GenerateString(Renamer.RenamerPhase.RenameMode.Ascii)));
                        meth.Body.Instructions.Insert(i + 3, Instruction.Create(OpCodes.Pop));
                        i += 3;
                    }
                }
            }
        }

        private static HashSet<Instruction> CollectTargets(IList<Instruction> instructions)
        {
            var targets = new HashSet<Instruction>();
            foreach (var instruction in instructions)
            {
                if (instruction.Operand is Instruction target)
                    targets.Add(target);
                else if (instruction.Operand is IList<Instruction> manyTargets)
                {
                    foreach (var branchTarget in manyTargets)
                        targets.Add(branchTarget);
                }
            }

            return targets;
        }

        private static bool HasComplexControlFlow(IList<Instruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.OpCode.FlowControl == FlowControl.Branch ||
                    instruction.OpCode.FlowControl == FlowControl.Cond_Branch ||
                    instruction.OpCode.FlowControl == FlowControl.Throw)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryCollectZeroDepthEntries(IList<Instruction> instructions, out HashSet<Instruction> zeroDepthEntries)
        {
            zeroDepthEntries = new HashSet<Instruction>();
            var depth = 0;

            foreach (var instruction in instructions)
            {
                if (depth == 0)
                    zeroDepthEntries.Add(instruction);

                instruction.CalculateStackUsage(out var pushes, out var pops);
                if (pushes < 0 || pops < 0 || depth < pops)
                    return false;

                depth = depth - pops + pushes;
            }

            return depth == 0;
        }
    }
}