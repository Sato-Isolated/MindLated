using dnlib.DotNet;
using dnlib.DotNet.Emit;
using MindLated.Protection.Arithmetic.Functions;
using MindLated.Protection.Arithmetic.Utils;
using System;
using System.Collections.Generic;

namespace MindLated.Protection.Arithmetic
{
    public static class Arithmetic
    {
        private static ModuleDef _moduleDef1 = null!;

        private static readonly List<Function> Tasks = new()
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

        public static void Execute(ModuleDef module)
        {
            _moduleDef1 = module;
            var generator = new Generator.Generator();
            foreach (var type in module.Types)
            {
                foreach (var meth in type.Methods)
                {
                    if (!meth.HasBody) continue;
                    if (meth.DeclaringType.IsGlobalModuleType) continue;
                    for (var i = 0; i < meth.Body.Instructions.Count; i++)
                    {
                        if (!ArithmeticUtils.CheckArithmetic(meth.Body.Instructions[i])) continue;
                        if (meth.Body.Instructions[i].GetLdcI4Value() < 0)
                        {
                            var iFunction = Tasks[generator.Next(5)];
                            var lstInstr = GenerateBody(iFunction.Arithmetic(meth.Body.Instructions[i], module));
                            meth.Body.Instructions[i].OpCode = OpCodes.Nop;
                            foreach (var instr in lstInstr)
                            {
                                meth.Body.Instructions.Insert(i + 1, instr);
                                i++;
                            }
                        }
                        else
                        {
                            var iFunction = Tasks[generator.Next(Tasks.Count)];
                            var lstInstr = GenerateBody(iFunction.Arithmetic(meth.Body.Instructions[i], module));
                            meth.Body.Instructions[i].OpCode = OpCodes.Nop;
                            foreach (var instr in lstInstr)
                            {
                                meth.Body.Instructions.Insert(i + 1, instr);
                                i++;
                            }
                        }
                    }
                }
            }
        }

        private static List<Instruction> GenerateBody(ArithmeticVt arithmeticVTs)
        {
            var instr = new List<Instruction>();
            if (IsArithmetic(arithmeticVTs.GetArithmetic()))
            {
                instr.Add(new Instruction(OpCodes.Ldc_R8, arithmeticVTs.GetValue().GetX()));
                instr.Add(new Instruction(OpCodes.Ldc_R8, arithmeticVTs.GetValue().GetY()));

                if (arithmeticVTs.GetToken().GetOperand() != null)
                {
                    instr.Add(new Instruction(OpCodes.Call, arithmeticVTs.GetToken().GetOperand()));
                }
                instr.Add(new Instruction(arithmeticVTs.GetToken().GetOpCode()));
                instr.Add(new Instruction(OpCodes.Call, _moduleDef1.Import(typeof(Convert).GetMethod("ToInt32", new[] { typeof(double) }))));
                //instructions.Add(new Instruction(OpCodes.Conv_I4));
            }
            else if (IsXor(arithmeticVTs.GetArithmetic()))
            {
                instr.Add(new Instruction(OpCodes.Ldc_I4, (int)arithmeticVTs.GetValue().GetX()));
                instr.Add(new Instruction(OpCodes.Ldc_I4, (int)arithmeticVTs.GetValue().GetY()));
                instr.Add(new Instruction(arithmeticVTs.GetToken().GetOpCode()));
                instr.Add(new Instruction(OpCodes.Conv_I4));
            }
            return instr;
        }

        private static bool IsArithmetic(ArithmeticTypes arithmetic)
        {
            return arithmetic is
                ArithmeticTypes.Add or
                ArithmeticTypes.Sub or
                ArithmeticTypes.Div or
                ArithmeticTypes.Mul or
                ArithmeticTypes.Abs or
                ArithmeticTypes.Log or
                ArithmeticTypes.Log10 or
                ArithmeticTypes.Truncate or
                ArithmeticTypes.Sin or
                ArithmeticTypes.Cos or
                ArithmeticTypes.Floor or
                ArithmeticTypes.Round or
                ArithmeticTypes.Tan or
                ArithmeticTypes.Tanh or
                ArithmeticTypes.Sqrt or
                ArithmeticTypes.Ceiling;
        }

        private static bool IsXor(ArithmeticTypes arithmetic)
        {
            return arithmetic == ArithmeticTypes.Xor;
        }
    }
}