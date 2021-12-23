using System;
using System.Collections.Generic;

namespace MindLated.Protection.Arithmetic
{
    public class ArithmeticEmulator
    {
        private readonly double _x;
        private readonly double _y;
        private readonly ArithmeticTypes _arithmeticTypes;
        public new ArithmeticTypes GetType { get; private set; }

        public ArithmeticEmulator(double x, double y, ArithmeticTypes arithmeticTypes)
        {
            _x = x;
            _y = y;
            _arithmeticTypes = arithmeticTypes;
        }

        public double GetValue()
        {
            return _arithmeticTypes switch
            {
                ArithmeticTypes.Add => _x - _y,
                ArithmeticTypes.Sub => _x + _y,
                ArithmeticTypes.Div => _x * _y,
                ArithmeticTypes.Mul => _x / _y,
                ArithmeticTypes.Xor => (int)_x ^ (int)_y,
                _ => -1
            };
        }

        public double GetValue(List<ArithmeticTypes> arithmetics)
        {
            var generator = new Generator.Generator();
            var arithmetic = arithmetics[generator.Next(arithmetics.Count)];
            GetType = arithmetic;
            return _arithmeticTypes switch
            {
                ArithmeticTypes.Abs => arithmetic switch
                {
                    ArithmeticTypes.Add => _x + Math.Abs(_y) * -1,
                    ArithmeticTypes.Sub => _x - Math.Abs(_y) * -1,
                    _ => -1
                },
                ArithmeticTypes.Log => arithmetic switch
                {
                    ArithmeticTypes.Add => _x - Math.Log(_y),
                    ArithmeticTypes.Sub => _x + Math.Log(_y),
                    _ => -1
                },
                ArithmeticTypes.Log10 => arithmetic switch
                {
                    ArithmeticTypes.Add => _x - Math.Log10(_y),
                    ArithmeticTypes.Sub => _x + Math.Log10(_y),
                    _ => -1
                },
                ArithmeticTypes.Sin => arithmetic switch
                {
                    ArithmeticTypes.Add => _x - Math.Sin(_y),
                    ArithmeticTypes.Sub => _x + Math.Sin(_y),
                    _ => -1
                },
                ArithmeticTypes.Cos => arithmetic switch
                {
                    ArithmeticTypes.Add => _x - Math.Cos(_y),
                    ArithmeticTypes.Sub => _x + Math.Cos(_y),
                    _ => -1
                },
                ArithmeticTypes.Floor => arithmetic switch
                {
                    ArithmeticTypes.Add => _x - Math.Floor(_y),
                    ArithmeticTypes.Sub => _x + Math.Floor(_y),
                    _ => -1
                },
                ArithmeticTypes.Round => arithmetic switch
                {
                    ArithmeticTypes.Add => _x - Math.Round(_y),
                    ArithmeticTypes.Sub => _x + Math.Round(_y),
                    _ => -1
                },
                ArithmeticTypes.Tan => arithmetic switch
                {
                    ArithmeticTypes.Add => _x - Math.Tan(_y),
                    ArithmeticTypes.Sub => _x + Math.Tan(_y),
                    _ => -1
                },
                ArithmeticTypes.Tanh => arithmetic switch
                {
                    ArithmeticTypes.Add => _x - Math.Tanh(_y),
                    ArithmeticTypes.Sub => _x + Math.Tanh(_y),
                    _ => -1
                },
                ArithmeticTypes.Sqrt => arithmetic switch
                {
                    ArithmeticTypes.Add => _x - Math.Sqrt(_y),
                    ArithmeticTypes.Sub => _x + Math.Sqrt(_y),
                    _ => -1
                },
                ArithmeticTypes.Ceiling => arithmetic switch
                {
                    ArithmeticTypes.Add => _x - Math.Ceiling(_y),
                    ArithmeticTypes.Sub => _x + Math.Ceiling(_y),
                    _ => -1
                },
                ArithmeticTypes.Truncate => arithmetic switch
                {
                    ArithmeticTypes.Add => _x - Math.Truncate(_y),
                    ArithmeticTypes.Sub => _x + Math.Truncate(_y),
                    _ => -1
                },
                _ => -1
            };
        }

        public double GetY() => _y;
    }
}