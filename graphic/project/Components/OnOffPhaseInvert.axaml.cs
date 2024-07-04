using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace project.Components
{
    public partial class OnOffPhaseInvert : UserControl
    {
        private Ellipse _button;
        private bool _isOn;
        public OnOffPhaseInvert()
        {
            InitializeComponent();

            _isOn = false;
            _button = this.FindControl<Ellipse>("OnOffPhaseInvertButton");
            _button.PointerPressed += SwitchingMode;
        }

        private void SwitchingMode(object sender, PointerEventArgs e)
        {
            if (_isOn == false)
            {
                _isOn = true;
                _button.Fill = Brushes.GreenYellow;
            }
            else
            {
                _isOn = false;
                _button.Fill = Brushes.Red;
            }
            MainWindow.Client.SendEnableEffect(2);
        }

    }
}