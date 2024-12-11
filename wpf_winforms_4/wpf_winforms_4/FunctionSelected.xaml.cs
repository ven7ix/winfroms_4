using System.Windows;
using System.Windows.Controls;

namespace wpf_winforms_4
{
    public partial class FunctionSelected : Window
    {
        public FunctionSelected()
        {
            InitializeComponent();
            InitializeTabs();
        }

        private void InitializeTabs()
        {
            TabItem tabSine = new TabItem();
            FunctionControl controlWaves = new FunctionControl(new Sine(), tabSine);
            tabSine.Header = "sine";
            tabSine.Content = controlWaves;
            tabsFunctions.Items.Add(tabSine);

            TabItem tabLinearExpression = new TabItem();
            FunctionControl controlLinearExpression = new FunctionControl(new LinearExpression(1, 1, 1), tabLinearExpression);
            tabLinearExpression.Header = "linear";
            tabLinearExpression.Content = controlLinearExpression;
            tabsFunctions.Items.Add(tabLinearExpression);

            TabItem tabWomanFunc = new TabItem();
            FunctionControl controlWomanFunc = new FunctionControl(new AtanDerivative(), tabWomanFunc);
            tabWomanFunc.Header = "atan derivative";
            tabWomanFunc.Content = controlWomanFunc;
            tabsFunctions.Items.Add(tabWomanFunc);
        }
    }
}