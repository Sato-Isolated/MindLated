using System;
using System.Collections.Generic;

namespace MindLated.Protection.Arithmetic
{
    public class ArithmeticEmulator
    {
        private readonly double x;
        private readonly double y;
        private readonly ArithmeticTypes arithmeticTypes;
        public new ArithmeticTypes GetType { get; private set; }

        public ArithmeticEmulator(double x, double y, ArithmeticTypes arithmeticTypes)
        {
            this.x = x;
            this.y = y;
            this.arithmeticTypes = arithmeticTypes;
        }

        public double GetValue()
        {
            switch (arithmeticTypes)
            {
                case ArithmeticTypes.Add:
                    return x - y;

                case ArithmeticTypes.Sub:
                    return x + y;

                case ArithmeticTypes.Div:
                    return x * y;

                case ArithmeticTypes.Mul:
                    return x / y;

                case ArithmeticTypes.Xor:
                    return ((int)x ^ (int)y);
            }
            return -1;
        }

        public double GetValue(List<ArithmeticTypes> arithmetics)
        {
            var generator = new Generator.Generator();
            var arithmetic = arithmetics[generator.Next(arithmetics.Count)];
            GetType = arithmetic;
            switch (arithmeticTypes)
            {
                case ArithmeticTypes.Abs:
                    switch (arithmetic)
                    {
                        case ArithmeticTypes.Add:
                            return x + (Math.Abs(y) * -1);

                        case ArithmeticTypes.Sub:
                            return x - (Math.Abs(y) * -1);
                    }
                    return -1;

                case ArithmeticTypes.Log:
                    switch (arithmetic)
                    {
                        case ArithmeticTypes.Add:
                            return x - (Math.Log(y));

                        case ArithmeticTypes.Sub:
                            return x + (Math.Log(y));
                    }
                    return -1;

                case ArithmeticTypes.Log10:
                    switch (arithmetic)
                    {
                        case ArithmeticTypes.Add:
                            return x - (Math.Log10(y));

                        case ArithmeticTypes.Sub:
                            return x + (Math.Log10(y));
                    }
                    return -1;

                case ArithmeticTypes.Sin:
                    switch (arithmetic)
                    {
                        case ArithmeticTypes.Add:
                            return x - (Math.Sin(y));

                        case ArithmeticTypes.Sub:
                            return x + (Math.Sin(y));
                    }
                    return -1;

                case ArithmeticTypes.Cos:
                    switch (arithmetic)
                    {
                        case ArithmeticTypes.Add:
                            return x - (Math.Cos(y));

                        case ArithmeticTypes.Sub:
                            return x + (Math.Cos(y));
                    }
                    return -1;

                case ArithmeticTypes.Floor:
                    switch (arithmetic)
                    {
                        case ArithmeticTypes.Add:
                            return x - (Math.Floor(y));

                        case ArithmeticTypes.Sub:
                            return x + (Math.Floor(y));
                    }
                    return -1;

                case ArithmeticTypes.Round:
                    switch (arithmetic)
                    {
                        case ArithmeticTypes.Add:
                            return x - (Math.Round(y));

                        case ArithmeticTypes.Sub:
                            return x + (Math.Round(y));
                    }
                    return -1;

                case ArithmeticTypes.Tan:
                    switch (arithmetic)
                    {
                        case ArithmeticTypes.Add:
                            return x - (Math.Tan(y));

                        case ArithmeticTypes.Sub:
                            return x + (Math.Tan(y));
                    }
                    return -1;

                case ArithmeticTypes.Tanh:
                    switch (arithmetic)
                    {
                        case ArithmeticTypes.Add:
                            return x - (Math.Tanh(y));

                        case ArithmeticTypes.Sub:
                            return x + (Math.Tanh(y));
                    }
                    return -1;

                case ArithmeticTypes.Sqrt:
                    switch (arithmetic)
                    {
                        case ArithmeticTypes.Add:
                            return x - (Math.Sqrt(y));

                        case ArithmeticTypes.Sub:
                            return x + (Math.Sqrt(y));
                    }
                    return -1;

                case ArithmeticTypes.Ceiling:
                    switch (arithmetic)
                    {
                        case ArithmeticTypes.Add:
                            return x - (Math.Ceiling(y));

                        case ArithmeticTypes.Sub:
                            return x + (Math.Ceiling(y));
                    }
                    return -1;

                case ArithmeticTypes.Truncate:
                    switch (arithmetic)
                    {
                        case ArithmeticTypes.Add:
                            return x - (Math.Truncate(y));

                        case ArithmeticTypes.Sub:
                            return x + (Math.Truncate(y));
                    }
                    return -1;
            }
            return -1;
        }

        public double GetY() => y;
    }
}