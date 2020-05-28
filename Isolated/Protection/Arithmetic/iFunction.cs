using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Isolated.Protection.Arithmetic
{
    public abstract class IFunction
    {
        public abstract ArithmeticTypes ArithmeticTypes { get; }

        public abstract ArithmeticVT Arithmetic(Instruction instruction, ModuleDef module);
    }
}