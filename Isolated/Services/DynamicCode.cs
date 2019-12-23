using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using OpCode = dnlib.DotNet.Emit.OpCode;
using OpCodes = dnlib.DotNet.Emit.OpCodes;
using ReflOpCodes = System.Reflection.Emit.OpCodes;

namespace Isolated.Services
{
    public class DynamicCode
    {
        private int intensity;

        private delegate int Result();

        private RandomGen r;

        public DynamicCode(int intensity)
        {
            this.intensity = intensity;
            r = new RandomGen();
        }

        public Instruction[] Create()
        {
            int positionValue = r.Next(0, intensity);
            List<Instruction> instructions = new List<Instruction>();
            instructions.Add(OpCodes.Ldc_I4.ToInstruction(r.Next()));
            instructions.Add(OpCodes.Ldc_I4.ToInstruction(r.Next()));
            for (int i = 0; i < intensity; i++)
            {
                instructions.Add(getRandomOperation().ToInstruction());
                if (positionValue == i)
                    instructions.Add(OpCodes.Ldarg_0.ToInstruction());
                else
                    instructions.Add(OpCodes.Ldc_I4.ToInstruction(r.Next()));
            }
            instructions.Add(getRandomOperation().ToInstruction());
            instructions.Add(OpCodes.Ret.ToInstruction());
            return instructions.ToArray();
        }

        public int RandomNumberInModule(Instruction[] instructions, int module, bool divisible)
        {
            int Rnum = module * r.Next(1, 12);
            Rnum = divisible ? Rnum : Rnum + 1;
            int x = 0;
            List<Instruction> instsx = new List<Instruction>();
            while (instructions[x].OpCode != OpCodes.Ldarg_0)
            {
                instsx.Add(instructions[x]);
                x++;
            }
            instsx.Add(OpCodes.Ret.ToInstruction());
            int valuesx = DynamicCode.Emulate(instsx.ToArray(), 0);
            List<Instruction> instdx = new List<Instruction>();
            instdx.Add(OpCodes.Ldc_I4.ToInstruction(Rnum));
            for (int i = instructions.Length - 2; i > x + 2; i -= 2)
            {
                Instruction operation = ReverseOperation(instructions[i].OpCode).ToInstruction();
                Instruction value = instructions[i - 1];
                instdx.Add(value);
                instdx.Add(operation);
            }
            instdx.Add(Instruction.Create(OpCodes.Ret));
            int valuedx = DynamicCode.Emulate(instdx.ToArray(), 0);
            Instruction ope = ReverseOperation(instructions[x + 1].OpCode).ToInstruction();
            List<Instruction> final = new List<Instruction>();
            final.Add(OpCodes.Ldc_I4.ToInstruction(valuedx));
            final.Add(OpCodes.Ldc_I4.ToInstruction(valuesx));
            final.Add(ope.OpCode == OpCodes.Add ? OpCodes.Sub.ToInstruction() : ope);
            final.Add(OpCodes.Ret.ToInstruction());
            int finalValue = DynamicCode.Emulate(final.ToArray(), 0);
            return ope.OpCode == OpCodes.Add ? (finalValue * -1) : finalValue;
        }

        public static int Emulate(Instruction[] code, int value)
        {
            DynamicMethod emulatore = new DynamicMethod("P61aWBY903203bRi", typeof(int), null);
            ILGenerator il = emulatore.GetILGenerator();
            foreach (Instruction instr in code)
            {
                if (instr.OpCode == OpCodes.Ldarg_0)
                    il.Emit(ReflOpCodes.Ldc_I4, value);
                else if (instr.Operand != null)
                    il.Emit(instr.OpCode.ToReflectionOp(), Convert.ToInt32(instr.Operand));
                else
                    il.Emit(instr.OpCode.ToReflectionOp());
            }
            Result ris = (Result)emulatore.CreateDelegate(typeof(Result));
            return ris.Invoke();
        }

        private OpCode getRandomOperation()
        {
            OpCode operation = null;
            switch (r.Next(0, 3))
            {
                case 0: operation = OpCodes.Add; break;
                case 1: operation = OpCodes.Sub; break;
                case 2: operation = OpCodes.Xor; break;
            }
            return operation;
        }

        private OpCode ReverseOperation(OpCode operation)
        {
            switch (operation.Code)
            {
                case Code.Add: return OpCodes.Sub;
                case Code.Sub: return OpCodes.Add;
                case Code.Xor: return OpCodes.Xor;
                default: throw new NotImplementedException();
            }
        }
    }
}