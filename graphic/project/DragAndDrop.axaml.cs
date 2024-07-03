using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using Avalonia.Controls.Shapes;

namespace project
{
    public partial class DragAndDrop : UserControl
    {
        private Ellipse _draggableEllipse;
        private bool _isDragging;
        private Point _startPoint;
        private double _initialTop;
        private const double MinY = -17.5;
        private const double MaxY = 182.5;

        public DragAndDrop()
        {
            InitializeComponent();
            _draggableEllipse = this.FindControl<Ellipse>("DraggableEllipse");
            _draggableEllipse.PointerPressed += DraggableEllipse_PointerPressed;
            _draggableEllipse.PointerReleased += DraggableEllipse_PointerReleased;
            _draggableEllipse.PointerMoved += DraggableEllipse_PointerMoved;

        }
        
        private void DraggableEllipse_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            _isDragging = true;
            _startPoint = e.GetPosition(this);
            _initialTop = Canvas.GetTop(_draggableEllipse);
        }
        
        private void DraggableEllipse_PointerReleased(object sender, PointerReleasedEventArgs e)
        {
            _isDragging = false;
        }

        private void DraggableEllipse_PointerMoved(object sender, PointerEventArgs e)
        {
            if (_isDragging)
            {
                var currentPosition = e.GetPosition(this);
                var offsetY = currentPosition.Y - _startPoint.Y;
                var newTop = _initialTop + offsetY;

                if (newTop < MinY) newTop = MinY;
                if (newTop > MaxY) newTop = MaxY;
                Canvas.SetTop(_draggableEllipse, newTop);
            }
        }
        
    }
}
