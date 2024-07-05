using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using System.ComponentModel;
using System.Linq;


public class HalfValueConverterOut : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double doubleValue)
        {
            return doubleValue / 2;
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

namespace project.Components
{
    public partial class MeeterOutput : UserControl, INotifyPropertyChanged
    {
        private double _leftRectangleHeight = 225;
        private double _rightRectangleHeight = 225;

        public event PropertyChangedEventHandler PropertyChanged;

        public double LeftRectangleHeight
        {
            get => _leftRectangleHeight;
            set
            {
                _leftRectangleHeight = value;
                OnPropertyChanged(nameof(LeftRectangleHeight));
            }
        }

        public double RightRectangleHeight
        {
            get => _rightRectangleHeight;
            set
            {
                _rightRectangleHeight = value;
                OnPropertyChanged(nameof(RightRectangleHeight));
            }
        }

        public MeeterOutput()
        {
            InitializeComponent();
            DataContext = this;
            MainWindow.Client.UpdateOutputMeterEventHandler += AdjustRectangleHeights;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public void AdjustRectangleHeights(object sender, byte[] buffer)
        {
            if (buffer.Length < 4)
                return;
            var floatsLeft = new float[512 / 2];
            var floatsRight = new float[512 / 2];
            for (var i = 0; i < 512; i++)
            {
                if (i % 2 == 0)
                    floatsLeft[i / 2] = BitConverter.ToSingle(buffer, i * 4);
                else
                    floatsRight[i / 2] = BitConverter.ToSingle(buffer, i * 4);
            }

            var Left = floatsLeft.Max(x => Math.Abs(x));
            var Right = floatsRight.Max(x => Math.Abs(x));
            
            _buffersLeft.Enqueue(Left);
            _buffersRight.Enqueue(Right);
            if (_buffersLeft.Count > 3)
                _buffersLeft.Dequeue();
            if (_buffersRight.Count > 3)
                _buffersRight.Dequeue();

            LeftRectangleHeight = (_buffersLeft.Average() * 225 - 225) * -1;
            RightRectangleHeight = (_buffersRight.Average() * 225 - 225) * -1;
        }
        
        private readonly Queue<float> _buffersLeft = [];
        private readonly Queue<float> _buffersRight = [];
    }

    public class HalfValueConverterOut : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                return doubleValue / 2;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
}