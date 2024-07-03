using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace project.Components
{
   
    public partial class RotateButton : UserControl
    {
        private Components.Button _rotatableComponent;
        private Point _previousMousePosition;
        private double _currentAngle;
        public RotateButton()
        {
            InitializeComponent();
            
            _rotatableComponent = this.FindControl<Components.Button>("MyButton");
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

                // Calculate angle based on mouse movement
                double angleDelta = delta.X; // You can adjust this for a different sensitivity
                _currentAngle += angleDelta;
                Console.WriteLine(_currentAngle);
                
                if (_currentAngle < -90)
                    _currentAngle = -90;
                else if (_currentAngle > 90)
                    _currentAngle = 90;

                // Apply the rotation
                RotateTransform rotateTransform = new RotateTransform(_currentAngle, 0, 0);
                _rotatableComponent.RenderTransform = rotateTransform;
                _previousMousePosition = currentPosition;

            }
        }
    }
}