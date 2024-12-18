using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpf_winforms_4
{
    internal class Squared : IFunction
    {
        public double a;
        public double b;
        public double c;
        public double d;
        public double e;
        public double f;

        public Squared(double a, double b, double c, double d, double e, double f)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
            this.e = e;
            this.f = f;
        }

        public double Calculate(double x, double y)
        {
            //ax^2 + by^2 + cxy + dx + ey + f
            return a * x * x + b * y * y + c * x * y + d * x + e * y + f;
        }
    }
}
