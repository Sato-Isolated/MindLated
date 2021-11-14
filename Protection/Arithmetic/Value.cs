namespace MindLated.Protection.Arithmetic
{
    public class Value
    {
        private readonly double _x;
        private readonly double _y;

        public Value(double x, double y)
        {
            _x = x;
            _y = y;
        }

        public double GetX() => _x;

        public double GetY() => _y;
    }
}