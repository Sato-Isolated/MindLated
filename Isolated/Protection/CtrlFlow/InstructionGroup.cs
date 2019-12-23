using dnlib.DotNet.Emit;
using System.Collections.Generic;

namespace Isolated.Protection.CtrlFlow
{
    public class InstructionGroup : List<Instruction>
    {
        public int ID;

        public int nextGroup;
    }
}