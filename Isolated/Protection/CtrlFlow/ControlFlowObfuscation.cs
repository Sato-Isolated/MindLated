using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Isolated.Protection.CtrlFlow
{
    internal class ControlFlowObfuscation
    {
        public static ModuleDef Module { get; set; }

        public static void Execute(ModuleDefMD md)
        {
            for (int x = 0; x < md.Types.Count; x++)
            {
                var tDef = md.Types[x];
                if (tDef != md.GlobalType)
                    for (int i = 0; i < tDef.Methods.Count; i++)
                    {
                        var mDef = tDef.Methods[i];
                        if (!mDef.Name.StartsWith("get_") && !mDef.Name.StartsWith("set_"))
                        {
                            if (!mDef.HasBody || mDef.IsConstructor) continue;
                            mDef.Body.SimplifyBranches();
                            ExecuteMethod(mDef);
                        }
                    }
            }
        }

        public static void ExecuteMethod(MethodDef method)
        {
            method.Body.SimplifyMacros(method.Parameters);
            List<Block> blocks = BlockParser.ParseMethod(method);
            blocks = Randomize(blocks);
            method.Body.Instructions.Clear();
            Local local = new Local(method.Module.CorLibTypes.Int32);
            method.Body.Variables.Add(local);
            Instruction target = Instruction.Create(OpCodes.Nop);
            Instruction instr = Instruction.Create(OpCodes.Br, target);
            foreach (Instruction instruction in Calc(0))
                method.Body.Instructions.Add(instruction);
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Stloc, local));
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Br, instr));
            method.Body.Instructions.Add(target);
            foreach (Block block in blocks)
            {
                if (block != blocks.Single(x => x.Number == blocks.Count - 1))
                {
                    method.Body.Instructions.Add(Instruction.Create(OpCodes.Ldloc, local));
                    foreach (Instruction instruction in Calc(block.Number))
                        method.Body.Instructions.Add(instruction);
                    method.Body.Instructions.Add(Instruction.Create(OpCodes.Ceq));
                    Instruction instruction4 = Instruction.Create(OpCodes.Nop);
                    method.Body.Instructions.Add(Instruction.Create(OpCodes.Brfalse, instruction4));

                    foreach (Instruction instruction in block.Instructions)
                        method.Body.Instructions.Add(instruction);

                    foreach (Instruction instruction in Calc(block.Number + 1))
                        method.Body.Instructions.Add(instruction);

                    method.Body.Instructions.Add(Instruction.Create(OpCodes.Stloc, local));
                    method.Body.Instructions.Add(instruction4);
                }
            }
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Ldloc, local));
            foreach (Instruction instruction in Calc(blocks.Count - 1))
                method.Body.Instructions.Add(instruction);
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Ceq));
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Brfalse, instr));
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Br, blocks.Single(x => x.Number == blocks.Count - 1).Instructions[0]));
            method.Body.Instructions.Add(instr);

            foreach (Instruction lastBlock in blocks.Single(x => x.Number == blocks.Count - 1).Instructions)
                method.Body.Instructions.Add(lastBlock);
        }

        public static Random rnd = new Random();

        public static List<Block> Randomize(List<Block> input)
        {
            List<Block> ret = new List<Block>();
            foreach (var group in input)
                ret.Insert(rnd.Next(0, ret.Count), group);
            return ret;
        }

        public static List<Instruction> Calc(int value)
        {
            List<Instruction> instructions = new List<Instruction> { Instruction.Create(OpCodes.Ldc_I4, value) };
            return instructions;
        }

        public void AddJump(IList<Instruction> instrs, Instruction target)
        {
            instrs.Add(Instruction.Create(OpCodes.Br, target));
        }
    }
}