using dnlib.DotNet.Emit;

namespace Isolated.Protection.Arithmetic
{
    public class Token
    {
        private readonly OpCode opCode;
        private readonly object Operand;

        public Token(OpCode opCode, object Operand)
        {
            this.opCode = opCode;
            this.Operand = Operand;
        }

        public Token(OpCode opCode)
        {
            this.opCode = opCode;
            this.Operand = null;
        }

        public OpCode GetOpCode() => opCode;

        public object GetOperand() => Operand;
    }
}