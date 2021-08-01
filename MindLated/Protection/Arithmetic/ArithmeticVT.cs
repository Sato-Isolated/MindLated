namespace MindLated.Protection.Arithmetic
{
    public class ArithmeticVt
    {
        private readonly Value _value;
        private readonly Token _token;
        private readonly ArithmeticTypes _arithmeticTypes;

        public ArithmeticVt(Value value, Token token, ArithmeticTypes arithmeticTypes)
        {
            _value = value;
            _token = token;
            _arithmeticTypes = arithmeticTypes;
        }

        public Value GetValue() => _value;

        public Token GetToken() => _token;

        public ArithmeticTypes GetArithmetic() => _arithmeticTypes;
    }
}