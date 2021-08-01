using dnlib.DotNet.Emit;

namespace MindLated.Protection.Arithmetic
{
    public class Token
    {
        private readonly OpCode _opCode;
        private readonly object _operand;

        public Token(OpCode opCode, object operand)
        {
            _opCode = opCode;
            _operand = operand;
        }

        public Token(OpCode opCode)
        {
            _opCode = opCode;
            _operand = null;
        }

        public OpCode GetOpCode() => _opCode;

        public object GetOperand() => _operand;
    }
}