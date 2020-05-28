using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Isolated.Protection.Arithmetic.Functions;
using Isolated.Protection.Arithmetic.Utils;
using System;
using System.Collections.Generic;

namespace Isolated.Protection.Arithmetic
{
    public class Arithmetic
    {
        public static ModuleDef moduleDef1;

        public static List<IFunction> Tasks = new List<IFunction>()
        {
            new Add(),
            new Sub(),
            new Div(),
            new Mul(),
            new Xor(),
            new Functions.Maths.Abs(),
            new Functions.Maths.Log(),
            new Functions.Maths.Log10(),
            new Functions.Maths.Sin(),
            new Functions.Maths.Cos(),
            new Functions.Maths.Floor(),
            new Functions.Maths.Round(),
            new Functions.Maths.Tan(),
            new Functions.Maths.Tanh(),
            new Functions.Maths.Sqrt(),
            new Functions.Maths.Ceiling(),
            new Functions.Maths.Truncate()
        };

        public static void Execute(ModuleDef moduleDef)
        {
            moduleDef1 = moduleDef;
            Generator.Generator generator = new Generator.Generator();
            foreach (TypeDef tDef in moduleDef.Types)
            {
                foreach (MethodDef mDef in tDef.Methods)
                {
                    if (!mDef.HasBody) continue;
                    if (mDef.DeclaringType.IsGlobalModuleType) continue;
                    for (int i = 0; i < mDef.Body.Instructions.Count; i++)
                    {
                        if (ArithmeticUtils.CheckArithmetic(mDef.Body.Instructions[i]))
                        {
                            if (mDef.Body.Instructions[i].GetLdcI4Value() < 0)
                            {
                                IFunction iFunction = Tasks[generator.Next(5)];
                                List<Instruction> lstInstr = GenerateBody(iFunction.Arithmetic(mDef.Body.Instructions[i], moduleDef));
                                if (lstInstr == null) continue;
                                mDef.Body.Instructions[i].OpCode = OpCodes.Nop;
                                foreach (Instruction instr in lstInstr)
                                {
                                    mDef.Body.Instructions.Insert(i + 1, instr);
                                    i++;
                                }
                            }
                            else
                            {
                                IFunction iFunction = Tasks[generator.Next(Tasks.Count)];
                                List<Instruction> lstInstr = GenerateBody(iFunction.Arithmetic(mDef.Body.Instructions[i], moduleDef));
                                if (lstInstr == null) continue;
                                mDef.Body.Instructions[i].OpCode = OpCodes.Nop;
                                foreach (Instruction instr in lstInstr)
                                {
                                    mDef.Body.Instructions.Insert(i + 1, instr);
                                    i++;
                                }
                            }
                        }
                    }
                }
            }
        }

        private static List<Instruction> GenerateBody(ArithmeticVT arithmeticVTs)
        {
            List<Instruction> instructions = new List<Instruction>();
            if (IsArithmetic(arithmeticVTs.GetArithmetic()))
            {
                instructions.Add(new Instruction(OpCodes.Ldc_R8, arithmeticVTs.GetValue().GetX()));
                instructions.Add(new Instruction(OpCodes.Ldc_R8, arithmeticVTs.GetValue().GetY()));

                if (arithmeticVTs.GetToken().GetOperand() != null)
                {
                    instructions.Add(new Instruction(OpCodes.Call, arithmeticVTs.GetToken().GetOperand()));
                }
                instructions.Add(new Instruction(arithmeticVTs.GetToken().GetOpCode()));
                instructions.Add(new Instruction(OpCodes.Call, moduleDef1.Import(typeof(Convert).GetMethod("ToInt32", new Type[] { typeof(double) }))));
                //instructions.Add(new Instruction(OpCodes.Conv_I4));
            }
            else if (IsXor(arithmeticVTs.GetArithmetic()))
            {
                instructions.Add(new Instruction(OpCodes.Ldc_I4, (int)arithmeticVTs.GetValue().GetX()));
                instructions.Add(new Instruction(OpCodes.Ldc_I4, (int)arithmeticVTs.GetValue().GetY()));
                instructions.Add(new Instruction(arithmeticVTs.GetToken().GetOpCode()));
                instructions.Add(new Instruction(OpCodes.Conv_I4));
            }
            return instructions;
        }

        private static bool IsArithmetic(ArithmeticTypes arithmetic)
        {
            return arithmetic == ArithmeticTypes.Add || arithmetic == ArithmeticTypes.Sub || arithmetic == ArithmeticTypes.Div || arithmetic == ArithmeticTypes.Mul ||
                arithmetic == ArithmeticTypes.Abs || arithmetic == ArithmeticTypes.Log || arithmetic == ArithmeticTypes.Log10 || arithmetic == ArithmeticTypes.Truncate ||
                arithmetic == ArithmeticTypes.Sin || arithmetic == ArithmeticTypes.Cos || arithmetic == ArithmeticTypes.Floor || arithmetic == ArithmeticTypes.Round ||
                arithmetic == ArithmeticTypes.Tan || arithmetic == ArithmeticTypes.Tanh || arithmetic == ArithmeticTypes.Sqrt || arithmetic == ArithmeticTypes.Ceiling;
        }

        private static bool IsXor(ArithmeticTypes arithmetic)
        {
            return arithmetic == ArithmeticTypes.Xor;
        }
    }
}