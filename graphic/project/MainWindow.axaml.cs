using System;
using System.Linq;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Input;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;

namespace project;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        KeyDown += MainWindow_KeyDown!;

        FirstRun();
        Client.AudioBufferEvent += OnRunSimulation;
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        Client.Stop();
        base.OnClosing(e);
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
            WindowState = WindowState.Normal;
            Width = 1920;
            Height = 1080;
            _isFullScreen = false;
        }
        else
        {
            WindowState = WindowState.FullScreen;
            _isFullScreen = true;
        }
    }

    private void FirstRun()
    {
        for (var i = 0; i < RoomWidth; i++)
        for (var j = 0; j < RoomHeight; j++)
            _room[i, j] = new Box();

        // Add walls in the room
        SelectLayout(1);

        AddAudioSource(RoomHeight / 2, RoomWidth / 3 * 2, 0);
        DrawRoom();
    }

    private void SelectLayout(int index)
    {
        switch (index)
        {
            case 1:
                for (var i = 0; i < RoomHeight / 2; i++)
                    _room[RoomHeight / 4 + i, RoomWidth / 4] = new Box { Type = BoxType.Wall };
                for (var i = 0; i < RoomWidth / 4; i++)
                    _room[RoomWidth / 4, RoomHeight / 2 + i] = new Box { Type = BoxType.Wall };
                for (var i = 0; i < RoomWidth / 4; i++)
                    _room[RoomWidth / 4 * 3 - 1, RoomHeight / 2 + i] = new Box { Type = BoxType.Wall };
                for (var i = 0; i < RoomHeight / 2; i++)
                    _room[RoomHeight / 4 + i, RoomWidth / 4 * 3] = new Box { Type = BoxType.Wall };
                break;
            case 2:
                for (var i = 0; i < RoomHeight / 2; i++)
                    _room[RoomHeight / 4 + i, RoomWidth / 4] = new Box { Type = BoxType.Air };
                for (var i = 0; i < RoomWidth / 4; i++)
                    _room[RoomWidth / 4, RoomHeight / 2 + i] = new Box { Type = BoxType.Wall };
                for (var i = 0; i < RoomWidth / 4; i++)
                    _room[RoomWidth / 4 * 3 - 1, RoomHeight / 2 + i] = new Box { Type = BoxType.Wall };
                for (var i = 0; i < RoomHeight / 2; i++)
                    _room[RoomHeight / 4 + i, RoomWidth / 4 * 3] = new Box { Type = BoxType.Wall };
                break;
        }
        DrawRoom();
    }

    private void OnRunSimulation(object? _, byte[] buffer)
    {
        if (buffer.Length < 4)
            return;

        var floats = new float[512];
        for (var i = 0; i < 512; i++)
            floats[i] = BitConverter.ToSingle(buffer, i * 4);

        var decibels = floats.Max(Math.Abs) * 300;
        if (decibels > 300)
            return;

        _buffers.Enqueue(decibels);
        if (_buffers.Count > 10)
            _buffers.Dequeue();
        foreach (var box in _room)
            box.Decibels = 0;
        foreach (var source in _audioSources)
            _room[source.Item1, source.Item2].Decibels = _buffers.Average();

        PropagateSound();
        DrawRoom();
    }

    private void AddAudioSource(int x, int y, double decibels = 100)
    {
        _room[x, y].Decibels = decibels;
        _room[x, y].Type = BoxType.Source;
        _audioSources.Add(new Tuple<int, int>(x, y));
    }

    private void PropagateSound()
    {
        Array.Copy(_room, _newRoom, _room.Length);

        foreach (var (x, y) in _audioSources)
            PropagateSoundFromSource(x, y, _newRoom);

        Array.Copy(_newRoom, _room, _room.Length);
    }

    private void PropagateSoundFromSource(int sourceX, int sourceY, Box[,] newRoom)
    {
        var queue = new Queue<Tuple<int, int>>();
        queue.Enqueue(new Tuple<int, int>(sourceX, sourceY));

        while (queue.Count > 0)
        {
            var (x, y) = queue.Dequeue();
            var decibels = newRoom[x, y].Decibels;
            if (decibels < 0.01)
                continue;

            for (var nx = x - 1; nx <= x + 1; nx++)
            for (var ny = y - 1; ny <= y + 1; ny++)
            {
                if (nx is < 0 or >= RoomWidth || ny is < 0 or >= RoomHeight || newRoom[nx, ny].Type == BoxType.Wall)
                    continue;

                var distance = Math.Sqrt((x - nx) * (x - nx) + (y - ny) * (y - ny));
                var newVolume = decibels - distance;
                if (newVolume < 0 || newVolume <= newRoom[nx, ny].Decibels)
                    continue;

                newRoom[nx, ny].Decibels = newVolume;
                queue.Enqueue(new Tuple<int, int>(nx, ny));

                if (newVolume > _maxDecibels)
                    _maxDecibels = newVolume;
                if (newVolume != 0 && newVolume < _minDecibels)
                    _minDecibels = newVolume;
            }
        }
    }

    private void DrawRoom()
    {
        var bitmap = new WriteableBitmap(new PixelSize(RoomSize, RoomSize), new Vector(96, 96), PixelFormat.Bgra8888,
            AlphaFormat.Premul);
        var decibelRange = _maxDecibels - _minDecibels;

        for (var i = 0; i < RoomWidth; i++)
        for (var j = 0; j < RoomHeight; j++)
        {
            var color = -1;
            if (_room[i, j].Type == BoxType.Air)
            {
                var normalizedDecibels = (_room[i, j].Decibels - _minDecibels) / decibelRange;
                normalizedDecibels = Math.Max(0, Math.Min(normalizedDecibels, 1));
                color = (int)(normalizedDecibels * 255);
            }

            for (var k = 0; k < PixelSize; k++)
            for (var l = 0; l < PixelSize; l++)
            {
                var index = (i * PixelSize + k) * RoomSize * 4 + (j * PixelSize + l) * 4;
                _pixels[index + 3] = 255;

                switch (_room[i, j].Type)
                {
                    case BoxType.Wall:
                        _pixels[index] = 0;
                        _pixels[index + 1] = 0;
                        _pixels[index + 2] = 0;
                        break;
                    case BoxType.Source:
                        _pixels[index] = 0;
                        _pixels[index + 1] = 255;
                        _pixels[index + 2] = 0;
                        break;
                    case BoxType.Air:
                    default:
                        _pixels[index] = (byte)(255 - color);
                        _pixels[index + 1] = 0;
                        _pixels[index + 2] = (byte)color;
                        break;
                }
            }
        }

        using (var bitmapLock = bitmap.Lock())
        {
            var ptr = bitmapLock.Address;
            System.Runtime.InteropServices.Marshal.Copy(_pixels, 0, ptr, _pixels.Length);
        }

        this.Find<Image>("SimulationImage")!.Source = bitmap;
    }
    
    public static readonly Client.Client Client = new();
    private bool _isFullScreen;

    private const int RoomWidth = 240;
    private const int RoomHeight = 240;
    private const int RoomSize = 721;
    private const int PixelSize = RoomSize / RoomWidth;

    private readonly Box[,] _room = new Box[RoomHeight, RoomWidth];
    private readonly Box[,] _newRoom = new Box[RoomHeight, RoomWidth];
    private readonly List<Tuple<int, int>> _audioSources = [];

    private readonly byte[] _pixels = new byte[RoomSize * RoomSize * 4];
    private readonly Queue<float> _buffers = [];

    private double _maxDecibels = 1;
    private double _minDecibels;

    private void ChangeScene(object? sender, RoutedEventArgs e)
    {
        var button = ( sender as Button )!;
        var value = int.Parse(button.Name![6].ToString());
        
        SelectLayout(value);
    }
}

public class Box
{
    public BoxType Type { get; set; } = BoxType.Air;
    public double Decibels { get; set; }
}

public enum BoxType
{
    Air,
    Wall,
    Source
}