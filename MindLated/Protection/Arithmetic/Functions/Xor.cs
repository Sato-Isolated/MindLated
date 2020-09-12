using dnlib.DotNet;
using dnlib.DotNet.Emit;
using MindLated.Protection.Arithmetic.Utils;

namespace MindLated.Protection.Arithmetic.Functions
{
    public class Xor : iFunction
    {
        public override ArithmeticTypes ArithmeticTypes => ArithmeticTypes.Xor;

        public override ArithmeticVT Arithmetic(Instruction instruction, ModuleDef module)
        {
            var generator = new Generator.Generator();
            if (!ArithmeticUtils.CheckArithmetic(instruction)) return null;
            var arithmeticEmulator = new ArithmeticEmulator(instruction.GetLdcI4Value(), generator.Next(), ArithmeticTypes);
            return (new ArithmeticVT(new Value(arithmeticEmulator.GetValue(), arithmeticEmulator.GetY()), new Token(OpCodes.Xor), ArithmeticTypes));
        }
    }
}