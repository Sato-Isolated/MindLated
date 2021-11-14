using System;

namespace MindLated.Protection.Arithmetic.Generator
{
    public class Generator
    {
        private readonly Random _random;

        public Generator()
        {
            _random = new Random(Guid.NewGuid().GetHashCode());
        }

        public int Next()
        {
            return _random.Next(int.MaxValue);
        }

        public int Next(int value)
        {
            return _random.Next(value);
        }

        public int Next(int min, int max)
        {
            return _random.Next(min, max);
        }
    }
}