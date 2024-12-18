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

            TabItem atanDerivative = new TabItem();
            FunctionControl controlAtanDerivative = new FunctionControl(new AtanDerivative(), atanDerivative);
            atanDerivative.Header = "atan derivative"; 
            atanDerivative.Content = controlAtanDerivative;
            tabsFunctions.Items.Add(atanDerivative);
        }
    }
}