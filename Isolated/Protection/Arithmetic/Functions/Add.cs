using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Isolated.Protection.Arithmetic.Utils;

namespace Isolated.Protection.Arithmetic.Functions
{
    public class Add : IFunction
    {
        public override ArithmeticTypes ArithmeticTypes => ArithmeticTypes.Add;

        public override ArithmeticVT Arithmetic(Instruction instruction, ModuleDef module)
        {
            if (!ArithmeticUtils.CheckArithmetic(instruction)) return null;
            var arithmeticEmulator = new ArithmeticEmulator(instruction.GetLdcI4Value(), ArithmeticUtils.GetY(instruction.GetLdcI4Value()), ArithmeticTypes);
            return (new ArithmeticVT(new Value(arithmeticEmulator.GetValue(), arithmeticEmulator.GetY()), new Token(OpCodes.Add), ArithmeticTypes));
        }
    }
}