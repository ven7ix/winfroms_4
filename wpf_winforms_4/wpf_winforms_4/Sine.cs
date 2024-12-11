using System;

namespace wpf_winforms_4
{
    internal class Sine : IFunction
    {
        public double Calculate(double x, double y)
        {
            return Math.Sin(x + y);
        }
    }
}