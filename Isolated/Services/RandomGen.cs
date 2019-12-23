using System;
using System.Security.Cryptography;

namespace Isolated.Services
{
    public class RandomGen
    {
        private const int BufferSize = 1024;
        private byte[] RandomBuffer;
        private int BufferOffset;
        private RNGCryptoServiceProvider rng;

        public RandomGen()
        {
            RandomBuffer = new byte[BufferSize];
            rng = new RNGCryptoServiceProvider();
            BufferOffset = RandomBuffer.Length;
        }

        private void FillBuffer()
        {
            rng.GetBytes(RandomBuffer);
            BufferOffset = 0;
        }

        public int Next()
        {
            if (BufferOffset >= RandomBuffer.Length)
            {
                FillBuffer();
            }
            int val = BitConverter.ToInt32(RandomBuffer, BufferOffset) & 0x7fffffff;
            BufferOffset += sizeof(int);
            return val;
        }

        public int Next(int maxValue)
        {
            return Next() % maxValue;
        }

        public int Next(int minValue, int maxValue)
        {
            if (maxValue < minValue)
            {
                throw new ArgumentOutOfRangeException("maxValue must be greater than or equal to minValue");
            }
            int range = maxValue - minValue;
            return minValue + Next(range);
        }

        public double NextDouble()
        {
            int val = Next();
            return (double)val / int.MaxValue;
        }

        public void GetBytes(byte[] buff)
        {
            rng.GetBytes(buff);
        }
    }
}