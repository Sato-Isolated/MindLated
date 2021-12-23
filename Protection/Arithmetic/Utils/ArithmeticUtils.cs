using dnlib.DotNet.Emit;
using System;
using System.Reflection;
using static MindLated.Protection.Arithmetic.ArithmeticTypes;

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

        public static MethodInfo? GetMethod(ArithmeticTypes mathType)
        {
            return mathType switch
            {
                Abs => typeof(Math).GetMethod("Abs", new[] { typeof(double) }),
                Round => typeof(Math).GetMethod("Round", new[] { typeof(double) }),
                Sin => typeof(Math).GetMethod("Sin", new[] { typeof(double) }),
                Cos => typeof(Math).GetMethod("Cos", new[] { typeof(double) }),
                Log => typeof(Math).GetMethod("Log", new[] { typeof(double) }),
                Log10 => typeof(Math).GetMethod("Log10", new[] { typeof(double) }),
                Sqrt => typeof(Math).GetMethod("Sqrt", new[] { typeof(double) }),
                Ceiling => typeof(Math).GetMethod("Ceiling", new[] { typeof(double) }),
                Floor => typeof(Math).GetMethod("Floor", new[] { typeof(double) }),
                Tan => typeof(Math).GetMethod("Tan", new[] { typeof(double) }),
                Tanh => typeof(Math).GetMethod("Tanh", new[] { typeof(double) }),
                Truncate => typeof(Math).GetMethod("Truncate", new[] { typeof(double) }),
                _ => null
            };
        }

        public static OpCode? GetOpCode(ArithmeticTypes arithmetic)
        {
            return arithmetic switch
            {
                Add => OpCodes.Add,
                Sub => OpCodes.Sub,
                _ => null
            };
        }
    }
}