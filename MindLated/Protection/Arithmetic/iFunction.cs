using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace MindLated.Protection.Arithmetic
{
    public abstract class iFunction
    {
        public abstract ArithmeticTypes ArithmeticTypes { get; }

        public abstract ArithmeticVT Arithmetic(Instruction instruction, ModuleDef module);
    }
}