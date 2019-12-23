using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using OpCode = dnlib.DotNet.Emit.OpCode;
using OpCodes = dnlib.DotNet.Emit.OpCodes;
using ReflOpCodes = System.Reflection.Emit.OpCodes;

namespace Isolated.Services
{
    public class Expression
    {
        private delegate int Result();

        private static RandomGen random = new RandomGen();
        public List<Instruction> Instructions { get; private set; }

        public static Expression CreateRandom(int intensity)
        {
            int positionValue = random.Next(0, intensity - 1);
            List<Instruction> instructions = new List<Instruction>();
            instructions.Add(OpCodes.Ldc_I4.ToInstruction(random.Next()));
            instructions.Add(OpCodes.Ldc_I4.ToInstruction(random.Next()));
            for (int i = 0; i < intensity; i++)
            {
                instructions.Add(GetRandomOperator().ToInstruction());
                if (positionValue == i)
                    instructions.Add(OpCodes.Ldarg_0.ToInstruction());
                else
                    instructions.Add(OpCodes.Ldc_I4.ToInstruction(random.Next()));
            }
            instructions.Add(GetRandomOperator().ToInstruction());
            instructions.Add(OpCodes.Ret.ToInstruction());
            return new Expression() { Instructions = instructions };
        }

        private static OpCode GetRandomOperator()
        {
            OpCode operation = null;
            switch (random.Next(0, 2))
            {
                case 0: operation = OpCodes.Add; break;
                case 1: operation = OpCodes.Sub; break;
                case 2: operation = OpCodes.Xor; break;
            }
            return operation;
        }

        private OpCode ReverseOperator(OpCode operation)
        {
            switch (operation.Code)
            {
                case Code.Add: return OpCodes.Sub;
                case Code.Sub: return OpCodes.Add;
                case Code.Xor: return OpCodes.Xor;
                default: throw new NotImplementedException();
            }
        }

        public void ReplaceValue(int index, int value)
        {
            Instructions.RemoveAt(index);
            Instructions.Insert(index, Instruction.CreateLdcI4(value));
        }

        public List<Instruction> ReplaceLdarg(int value)
        {
            List<Instruction> expression = new List<Instruction>();
            int length = Instructions.Count;
            for (int i = 0; i < length; i++)
            {
                Instruction instr = Instructions[i];
                if (instr.OpCode == OpCodes.Ldarg_0)
                    expression.Add(Instruction.CreateLdcI4(value));
                else
                    expression.Add(instr);
            }
            return expression;
        }

        private int Emulate(List<Instruction> emuInstr, int value)
        {
            DynamicMethod emulator = new DynamicMethod("A14dVgagVWs4i7jf", typeof(int), null);
            ILGenerator il = emulator.GetILGenerator();
            foreach (Instruction instr in emuInstr)
            {
                if (instr.OpCode == OpCodes.Ldarg_0)
                    il.Emit(ReflOpCodes.Ldc_I4, value);
                else if (instr.Operand != null)
                    il.Emit(instr.OpCode.ToReflectionOp(), Convert.ToInt32(instr.Operand));
                else
                    il.Emit(instr.OpCode.ToReflectionOp());
            }
            Result ris = (Result)emulator.CreateDelegate(typeof(Result));
            return ris.Invoke();
        }

        public int ResolveEquation(int value)
        {
            int x = 0;
            List<Instruction> instsx = new List<Instruction>();
            while (Instructions[x].OpCode != OpCodes.Ldarg_0)
            {
                instsx.Add(Instructions[x]);
                x++;
            }
            instsx.Add(OpCodes.Ret.ToInstruction());
            int valuesx = Emulate(instsx, 0);
            List<Instruction> instdx = new List<Instruction>();
            instdx.Add(OpCodes.Ldc_I4.ToInstruction(value));
            for (int i = Instructions.Count - 2; i > x + 2; i -= 2)
            {
                Instruction operation = ReverseOperator(Instructions[i].OpCode).ToInstruction();
                Instruction last = Instructions[i - 1];
                instdx.Add(last);
                instdx.Add(operation);
            }
            instdx.Add(Instruction.Create(OpCodes.Ret));
            int valuedx = Emulate(instdx, 0);
            Instruction ope = ReverseOperator(Instructions[x + 1].OpCode).ToInstruction();
            List<Instruction> final = new List<Instruction>();
            final.Add(OpCodes.Ldc_I4.ToInstruction(valuedx));
            final.Add(OpCodes.Ldc_I4.ToInstruction(valuesx));
            final.Add(ope.OpCode == OpCodes.Add ? OpCodes.Sub.ToInstruction() : ope);
            final.Add(OpCodes.Ret.ToInstruction());
            int finalValue = Emulate(final, 0);
            return ope.OpCode == OpCodes.Add ? (finalValue * -1) : finalValue;
        }
    }
}