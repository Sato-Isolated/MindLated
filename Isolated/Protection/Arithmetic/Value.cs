namespace Isolated.Protection.Arithmetic
{
    public class Value
    {
        private readonly double x;
        private readonly double y;

        public Value(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public double GetX() => x;

        public double GetY() => y;
    }
}