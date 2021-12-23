using dnlib.DotNet;
using dnlib.DotNet.Emit;
using MindLated.Protection.Arithmetic.Utils;

namespace MindLated.Protection.Arithmetic.Functions
{
    public class Xor : Function
    {
        public virtual ArithmeticTypes ArithmeticTypes => ArithmeticTypes.Xor;

        public override ArithmeticVt Arithmetic(Instruction instruction, ModuleDef module)
        {
            var generator = new Generator.Generator();
            if (!ArithmeticUtils.CheckArithmetic(instruction)) return null!;
            var arithmeticEmulator = new ArithmeticEmulator(instruction.GetLdcI4Value(), generator.Next(), ArithmeticTypes);
            return new ArithmeticVt(new Value(arithmeticEmulator.GetValue(), arithmeticEmulator.GetY()), new Token(OpCodes.Xor), ArithmeticTypes);
        }
    }
}