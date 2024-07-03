using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace project.Components
{
   
    public partial class ReverbButton : UserControl
    {
        private Components.Button _rotatableComponent;
        private Point _previousMousePosition;
        private double _currentAngle;
        
        public ReverbButton()
        {
            InitializeComponent();
            
            _rotatableComponent = this.FindControl<Components.Button>("ReverbButtonRotate");
            _rotatableComponent.PointerPressed += RotatableComponent_PointerPressed;
            _rotatableComponent.PointerMoved += RotatableComponent_PointerMoved;
            RotateTransform rotateTransform = new RotateTransform(-90, 0, 0);
            _rotatableComponent.RenderTransform = rotateTransform;
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
                    _currentAngle = 90;
                
                RotateTransform rotateTransform = new RotateTransform(_currentAngle, 0, 0);
                _rotatableComponent.RenderTransform = rotateTransform;
                _previousMousePosition = currentPosition;
                
                // -90 is 0 reverb, 90 is 19 reverb (short)
                var reverb = (_currentAngle + 90) / 180 * 19;
                var reverbFloat = (short) reverb;
                
                MainWindow.Client.SendReverbValue(reverbFloat);

            }
        }
    }
}