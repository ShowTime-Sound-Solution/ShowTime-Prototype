using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace project.Components
{
   
    public partial class GainButton : UserControl
    {
        private Components.Button _rotatableComponent;
        private Point _previousMousePosition;
        private double _currentAngle;
        public GainButton()
        {
            InitializeComponent();
            
            _rotatableComponent = this.FindControl<Components.Button>("GainButtonRotate");
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
                
                if (_currentAngle < -95)
                    _currentAngle = -95;
                else if (_currentAngle > 95)
                    _currentAngle = 95; // 190° pour les 19 de bass
                
                RotateTransform rotateTransform = new RotateTransform(_currentAngle, 0, 0);
                _rotatableComponent.RenderTransform = rotateTransform;
                _previousMousePosition = currentPosition;
            }
        }
    }
}