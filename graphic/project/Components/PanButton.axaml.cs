using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace project.Components
{
   
    public partial class PanButton : UserControl
    {
        private Components.Button _rotatableComponent;
        private Point _previousMousePosition;
        private double _currentAngle;
        
        public PanButton()
        {
            InitializeComponent();
            
            _rotatableComponent = this.FindControl<Components.Button>("PanButtonRotate");
            _rotatableComponent.PointerPressed += RotatableComponent_PointerPressed;
            _rotatableComponent.PointerMoved += RotatableComponent_PointerMoved;
        }

        private void RotatableComponent_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            _previousMousePosition = e.GetPosition(this);
        }

        private void RotatableComponent_PointerMoved(object sender, PointerEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                Point currentPosition = e.GetPosition(this);
                Vector delta = currentPosition - _previousMousePosition;

                double angleDelta = delta.X;
                _currentAngle += angleDelta;
                Console.WriteLine(_currentAngle);
                
                if (_currentAngle < -90)
                    _currentAngle = -90;
                else if (_currentAngle > 90)
                    _currentAngle = 90; // 190° pour les 19 de bass
                
                RotateTransform rotateTransform = new RotateTransform(_currentAngle, 0, 0);
                _rotatableComponent.RenderTransform = rotateTransform;
                _previousMousePosition = currentPosition;
                
                // -90 is -1 pan, 90 is 1 pan
                var pan = _currentAngle / 90 * -1;
                var panFloat = (float) pan;
                
                MainWindow.Client.SendPanValue(panFloat);
            }
        }
    }
}