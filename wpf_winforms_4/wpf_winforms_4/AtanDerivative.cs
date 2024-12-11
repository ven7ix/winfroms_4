namespace wpf_winforms_4
{
    internal class AtanDerivative : IFunction
    {
        public double Calculate(double x, double y)
        {
            return 1 / (1 + (x + y) * (x + y));
        }
    }
}