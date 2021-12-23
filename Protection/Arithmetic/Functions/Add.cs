using dnlib.DotNet;
using dnlib.DotNet.Emit;
using MindLated.Protection.Arithmetic.Utils;

namespace MindLated.Protection.Arithmetic.Functions
{
    public class Add : Function
    {
        public virtual ArithmeticTypes ArithmeticTypes => ArithmeticTypes.Add;

        public override ArithmeticVt Arithmetic(Instruction instruction, ModuleDef module)
        {
            if (!ArithmeticUtils.CheckArithmetic(instruction)) return null!;
            var arithmeticEmulator = new ArithmeticEmulator(instruction.GetLdcI4Value(), ArithmeticUtils.GetY(instruction.GetLdcI4Value()), ArithmeticTypes);
            return new ArithmeticVt(new Value(arithmeticEmulator.GetValue(), arithmeticEmulator.GetY()), new Token(OpCodes.Add), ArithmeticTypes);
        }
    }
}