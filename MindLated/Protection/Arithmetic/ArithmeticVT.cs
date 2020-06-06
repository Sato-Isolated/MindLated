namespace MindLated.Protection.Arithmetic
{
    public class ArithmeticVT
    {
        private readonly Value value;
        private readonly Token token;
        private readonly ArithmeticTypes arithmeticTypes;

        public ArithmeticVT(Value value, Token token, ArithmeticTypes arithmeticTypes)
        {
            this.value = value;
            this.token = token;
            this.arithmeticTypes = arithmeticTypes;
        }

        public Value GetValue() => value;

        public Token GetToken() => token;

        public ArithmeticTypes GetArithmetic() => arithmeticTypes;
    }
}