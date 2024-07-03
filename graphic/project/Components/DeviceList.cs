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
using project.Client;

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
        
        
     
        MainWindow.Client.AvailableDeviceEvent += ClientOnAvailableDeviceEvent;
    }

    private void ClientOnAvailableDeviceEvent(object sender, Dictionary<int, string> args)
    {
        this.FindControl<DataGrid>("DeviceGrid").ItemsSource = args;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void RefreshButton_OnClick(object? sender, RoutedEventArgs e)
    {
        MainWindow.Client.AskAvailableDevices();
    }

    private void DeviceGrid_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 0)
            return;
        Console.WriteLine(((KeyValuePair<int, string>)e.AddedItems[0]!).Key);
        MainWindow.Client.SelectDevice(((KeyValuePair<int, string>)e.AddedItems[0]!).Key);
    }
}
