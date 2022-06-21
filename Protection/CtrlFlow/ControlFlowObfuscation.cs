using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MindLated.Protection.CtrlFlow
{
    internal class ControlFlowObfuscation
    {
        public static void Execute(ModuleDefMD md)
        {
            foreach (var type in md.Types)
            {
                if (type == md.GlobalType) continue;
                foreach (var meth in type.Methods)
                {
                    if (meth.Name.StartsWith("get_") || meth.Name.StartsWith("set_")) continue;
                    if (!meth.HasBody || meth.IsConstructor) continue;
                    meth.Body.SimplifyBranches();
                    ExecuteMethod(meth);
                }
            }
        }

        private static void ExecuteMethod(MethodDef meth)
        {
            meth.Body.SimplifyMacros(meth.Parameters);
            var blocks = BlockParser.ParseMethod(meth);
            blocks = Randomize(blocks);
            meth.Body.Instructions.Clear();
            var local = new Local(meth.Module.CorLibTypes.Int32);
            meth.Body.Variables.Add(local);
            var target = Instruction.Create(OpCodes.Nop);
            var instr = Instruction.Create(OpCodes.Br, target);
            foreach (var instruction in Calc(0))
                meth.Body.Instructions.Add(instruction);
            meth.Body.Instructions.Add(Instruction.Create(OpCodes.Stloc, local));
            meth.Body.Instructions.Add(Instruction.Create(OpCodes.Br, instr));
            meth.Body.Instructions.Add(target);
            foreach (var block in blocks.Where(block => block != blocks.Single(x => x.Number == blocks.Count - 1)))
            {
                meth.Body.Instructions.Add(Instruction.Create(OpCodes.Ldloc, local));
                foreach (var instruction in Calc(block.Number))
                    meth.Body.Instructions.Add(instruction);
                meth.Body.Instructions.Add(Instruction.Create(OpCodes.Ceq));
                var instruction4 = Instruction.Create(OpCodes.Nop);
                meth.Body.Instructions.Add(Instruction.Create(OpCodes.Brfalse, instruction4));

                foreach (var instruction in block.Instructions)
                    meth.Body.Instructions.Add(instruction);

                foreach (var instruction in Calc(block.Number + 1))
                    meth.Body.Instructions.Add(instruction);

                meth.Body.Instructions.Add(Instruction.Create(OpCodes.Stloc, local));
                meth.Body.Instructions.Add(instruction4);
            }
            meth.Body.Instructions.Add(Instruction.Create(OpCodes.Ldloc, local));
            foreach (var instruction in Calc(blocks.Count - 1))
                meth.Body.Instructions.Add(instruction);
            meth.Body.Instructions.Add(Instruction.Create(OpCodes.Ceq));
            meth.Body.Instructions.Add(Instruction.Create(OpCodes.Brfalse, instr));
            meth.Body.Instructions.Add(Instruction.Create(OpCodes.Br, blocks.Single(x => x.Number == blocks.Count - 1).Instructions[0]));
            meth.Body.Instructions.Add(instr);

            foreach (var lastBlock in blocks.Single(x => x.Number == blocks.Count - 1).Instructions)
                meth.Body.Instructions.Add(lastBlock);
        }

        private static readonly Random Rnd = new();

        private static List<Block> Randomize(List<Block> input)
        {
            var ret = new List<Block>();
            foreach (var group in input)
                ret.Insert(Rnd.Next(0, ret.Count), group);
            return ret;
        }

        private static List<Instruction> Calc(int value)
        {
            var instructions = new List<Instruction> { Instruction.Create(OpCodes.Ldc_I4, value) };
            return instructions;
        }

        public void AddJump(IList<Instruction> instrs, Instruction target)
        {
            instrs.Add(Instruction.Create(OpCodes.Br, target));
        }
    }
}