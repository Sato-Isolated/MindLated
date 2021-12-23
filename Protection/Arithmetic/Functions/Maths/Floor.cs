using dnlib.DotNet;
using dnlib.DotNet.Emit;
using MindLated.Protection.Arithmetic.Utils;
using System.Collections.Generic;

namespace MindLated.Protection.Arithmetic.Functions.Maths
{
    public class Floor : Function
    {
        public virtual ArithmeticTypes ArithmeticTypes => ArithmeticTypes.Floor;

        public override ArithmeticVt Arithmetic(Instruction instruction, ModuleDef module)
        {
            if (!ArithmeticUtils.CheckArithmetic(instruction)) return null!;
            var arithmeticTypes = new List<ArithmeticTypes> { ArithmeticTypes.Add, ArithmeticTypes.Sub };
            var arithmeticEmulator = new ArithmeticEmulator(instruction.GetLdcI4Value(), ArithmeticUtils.GetY(instruction.GetLdcI4Value()), ArithmeticTypes);
            return new ArithmeticVt(new Value(arithmeticEmulator.GetValue(arithmeticTypes), arithmeticEmulator.GetY()), new Token(ArithmeticUtils.GetOpCode(arithmeticEmulator.GetType), module.Import(ArithmeticUtils.GetMethod(ArithmeticTypes))), ArithmeticTypes);
        }
    }
}