using System;
using System.Collections.Generic;

namespace Isolated.Protection.CtrlFlow
{
    public class InstructionGroups : List<InstructionGroup>
    {
        public void Scramble(out InstructionGroups incGroups)
        {
            InstructionGroups instructionGroups = new InstructionGroups();
            foreach (InstructionGroup item in this)
            {
                instructionGroups.Insert(InstructionGroups.r.Next(0, instructionGroups.Count), item);
            }
            incGroups = instructionGroups;
        }

        public InstructionGroup getGroup(int id)
        {
            foreach (InstructionGroup instructionGroup in this)
            {
                if (instructionGroup.ID == id)
                {
                    return instructionGroup;
                }
            }
            throw new Exception("Invalid ID!");
        }

        public InstructionGroup getLast()
        {
            return this.getGroup(base.Count - 1);
        }

        // Token: 0x04000003 RID: 3
        private static Random r = new Random();
    }
}