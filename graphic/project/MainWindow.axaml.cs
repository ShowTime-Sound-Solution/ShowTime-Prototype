using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia;

namespace project;

public partial class MainWindow : Window
{
    private bool _isFullScreen = false;
    public MainWindow()
    {
        InitializeComponent();
        this.KeyDown += MainWindow_KeyDown;
        var client = new Client.Client();
        
        
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