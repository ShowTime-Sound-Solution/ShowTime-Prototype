using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using project.Components;

namespace project;

public partial class MainWindow : Window
{
    private bool _isFullScreen = false;
    public static Client.Client Client = new Client.Client();

    public Dictionary<int, string> Test = new Dictionary<int, string>()
    {
        {1, "abc"},
        {2, "def"}
    };
    public MainWindow()
    {
        InitializeComponent();
        this.KeyDown += MainWindow_KeyDown;
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    private void MainWindow_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.F11)
        {
            ToggleFullScreen();
        }
    }

    private void ToggleFullScreen()
    {
        if (_isFullScreen)
        {
            this.WindowState = WindowState.Normal;
            this.Width = 1920;
            this.Height = 1080;
            _isFullScreen = false;
        }
        else
        {
            this.WindowState = WindowState.FullScreen;
            _isFullScreen = true;
        }
    }
}