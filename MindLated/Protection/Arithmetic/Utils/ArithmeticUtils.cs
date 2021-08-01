using dnlib.DotNet.Emit;
using System;

namespace MindLated.Protection.Arithmetic.Utils
{
    public static class ArithmeticUtils
    {
        public static bool CheckArithmetic(Instruction instruction)
        {
            if (!instruction.IsLdcI4())
                return false;
            if (instruction.GetLdcI4Value() == 1)
                return false;
            return instruction.GetLdcI4Value() != 0;
        }

        public static double GetY(double x) => x / 2;

        public static System.Reflection.MethodInfo GetMethod(ArithmeticTypes mathType)
        {
            return mathType switch
            {
                ArithmeticTypes.Abs => typeof(Math).GetMethod("Abs", new[] { typeof(double) }),
                ArithmeticTypes.Round => typeof(Math).GetMethod("Round", new[] { typeof(double) }),
                ArithmeticTypes.Sin => typeof(Math).GetMethod("Sin", new[] { typeof(double) }),
                ArithmeticTypes.Cos => typeof(Math).GetMethod("Cos", new[] { typeof(double) }),
                ArithmeticTypes.Log => typeof(Math).GetMethod("Log", new[] { typeof(double) }),
                ArithmeticTypes.Log10 => typeof(Math).GetMethod("Log10", new[] { typeof(double) }),
                ArithmeticTypes.Sqrt => typeof(Math).GetMethod("Sqrt", new[] { typeof(double) }),
                ArithmeticTypes.Ceiling => typeof(Math).GetMethod("Ceiling", new[] { typeof(double) }),
                ArithmeticTypes.Floor => typeof(Math).GetMethod("Floor", new[] { typeof(double) }),
                ArithmeticTypes.Tan => typeof(Math).GetMethod("Tan", new[] { typeof(double) }),
                ArithmeticTypes.Tanh => typeof(Math).GetMethod("Tanh", new[] { typeof(double) }),
                ArithmeticTypes.Truncate => typeof(Math).GetMethod("Truncate", new[] { typeof(double) }),
                _ => null
            };
        }

        public static OpCode GetOpCode(ArithmeticTypes arithmetic)
        {
            return arithmetic switch
            {
                ArithmeticTypes.Add => OpCodes.Add,
                ArithmeticTypes.Sub => OpCodes.Sub,
                _ => null
            };
        }
    }
}