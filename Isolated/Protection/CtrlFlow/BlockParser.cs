using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Collections.Generic;

namespace Isolated.Protection.CtrlFlow
{
    public class BlockParser
    {
        public static List<Block> ParseMethod(MethodDef method)
        {
            List<Block> blocks = new List<Block>();
            new List<Instruction>(method.Body.Instructions);
            Block block = new Block();
            int Id = 0;
            int usage = 0;
            block.Number = Id;
            block.Instructions.Add(Instruction.Create(OpCodes.Nop));
            blocks.Add(block);
            block = new Block();
            Stack<ExceptionHandler> handlers = new Stack<ExceptionHandler>();
            foreach (Instruction instruction in method.Body.Instructions)
            {
                foreach (var eh in method.Body.ExceptionHandlers)
                {
                    if (eh.HandlerStart == instruction || eh.TryStart == instruction || eh.FilterStart == instruction)
                        handlers.Push(eh);
                }
                foreach (var eh in method.Body.ExceptionHandlers)
                {
                    if (eh.HandlerEnd == instruction || eh.TryEnd == instruction)
                        handlers.Pop();
                }

                instruction.CalculateStackUsage(out var stacks, out var pops);
                block.Instructions.Add(instruction);
                usage += stacks - pops;
                if (stacks == 0)
                {
                    if (instruction.OpCode != OpCodes.Nop)
                    {
                        if ((usage == 0 || instruction.OpCode == OpCodes.Ret) && handlers.Count == 0)
                        {
                            block.Number = ++Id;
                            blocks.Add(block);
                            block = new Block();
                        }
                    }
                }
            }
            return blocks;
        }
    }
}