using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace project.Components;

public partial class DeviceList : UserControl
{
    
    public static readonly StyledProperty<Dictionary<int, string>> DevicesProperty =
        AvaloniaProperty.Register<DeviceList, Dictionary<int, string>>(nameof(Devices), defaultValue: new Dictionary<int, string>());

    public Dictionary<int, string> Devices
    {
        get => GetValue(DevicesProperty);
        set => SetValue(DevicesProperty, value);
    }
    public DeviceList()
    {
        InitializeComponent();
        var grid = this.FindControl<DataGrid>("DeviceGrid");
        
        
        
        MainWindow.Client.Devices.CollectionChanged += CollChanged;
    }
    
    private void CollChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        this.FindControl<DataGrid>("DeviceGrid").ItemsSource = MainWindow.Client.Devices.ToDictionary(x => x.Key, x => x.Value);
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void DeviceGrid_OnLoaded(object? sender, RoutedEventArgs e)
    {
        this.FindControl<DataGrid>("DeviceGrid").ItemsSource = MainWindow.Client.Devices;

    }

    private void RefreshButton_OnClick(object? sender, RoutedEventArgs e)
    {
        MainWindow.Client.AskAvailableDevices();
    }
}
