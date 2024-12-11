namespace wpf_winforms_4
{
    internal class LinearExpression : IFunction
    {
        private readonly double a;
        private readonly double b;
        private readonly double c;

        public LinearExpression(double a, double b, double c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }

        public double Calculate(double x, double y)
        {
            return a * x + b * y + c;
        }
    }
}