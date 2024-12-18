using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpf_winforms_4
{
    internal class Abs : IFunction
    {
        public double a;
        public double b;
        public double c;
        public double d;
        public double e;

        public Abs(double a, double b, double c, double d, double e)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
            this.e = e;
        }

        public Abs(Abs funciton)
        {
            a = funciton.a;
            b = funciton.b;
            c = funciton.c;
            d = funciton.d;
            e = funciton.e;
        }

        public double Calculate(double x, double y)
        {
            return a * Math.Abs(x - b) + c * Math.Abs(y - d) + e;
        }
    }
}
