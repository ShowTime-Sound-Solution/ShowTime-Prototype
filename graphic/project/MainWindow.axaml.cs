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
using project.Client;

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

        AddAudioSource(RoomHeight / 3 * 2, RoomWidth / 3 * 2, 0);
        AddAudioSource(RoomHeight / 3 * 1, RoomWidth / 3 * 2, 0);
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
        var floatsLeft = new float[512 / 2];
        var floatsRight = new float[512 / 2];
        for (var i = 0; i < 512; i++)
        {
            if (i % 2 == 0)
                floatsLeft[i / 2] = BitConverter.ToSingle(buffer, i * 4);
            else
                floatsRight[i / 2] = BitConverter.ToSingle(buffer, i * 4);
        }

        var decibelsLeft = floatsLeft.Max(x => Math.Abs(x)) * 100;
        var decibelsRight = floatsRight.Max(x => Math.Abs(x)) * 100;
        if (decibelsLeft > 300 || decibelsRight > 300)
            return;

        _buffersLeft.Enqueue(decibelsLeft);
        _buffersRight.Enqueue(decibelsRight);
        if (_buffersLeft.Count > 5)
            _buffersLeft.Dequeue();
        if (_buffersRight.Count > 5)
            _buffersRight.Dequeue();

        foreach (var box in _room)
            box.Decibels.Clear();
        _room[_audioSources[0].Item1, _audioSources[0].Item2].Decibels[0] = _buffersRight.Average();
        _room[_audioSources[1].Item1, _audioSources[1].Item2].Decibels[1] = _buffersLeft.Average();

        // var now = DateTime.Now;
        PropagateSound();
        // Console.WriteLine((DateTime.Now - now).TotalMilliseconds);
        DrawRoom();
    }

    private void AddAudioSource(int x, int y, double decibels = 100)
    {
        _audioSources.Add(new Tuple<int, int>(x, y));
        _room[x, y].Decibels[_audioSources.Count - 1] = decibels;
        _room[x, y].Type = BoxType.Source;
    }

    private void PropagateSound()
    {
        Array.Copy(_room, _newRoom, _room.Length);

        for (var i = 0; i < _audioSources.Count; i++)
            PropagateSoundFromSource(i, _newRoom);

        Array.Copy(_newRoom, _room, _room.Length);
    }

    private void PropagateSoundFromSource(int sourceIndex, Box[,] newRoom)
    {
        var queue = new Queue<Tuple<int, int>>();
        queue.Enqueue(new Tuple<int, int>(_audioSources[sourceIndex].Item1, _audioSources[sourceIndex].Item2));

        while (queue.Count > 0)
        {
            var (x, y) = queue.Dequeue();
            var decibels = newRoom[x, y].Decibels[sourceIndex];
            if (decibels < 0.01)
                continue;

            for (var nx = x - 1; nx <= x + 1; nx++)
            for (var ny = y - 1; ny <= y + 1; ny++)
            {
                if (nx is < 0 or >= RoomWidth || ny is < 0 or >= RoomHeight || newRoom[nx, ny].Type == BoxType.Wall)
                    continue;

                var distance = Math.Sqrt((x - nx) * (x - nx) + (y - ny) * (y - ny));
                var newVolume = decibels - distance * 1.2;
                if (newVolume < 0 ||
                    (newRoom[nx, ny].Decibels.TryGetValue(sourceIndex, out var value) && newVolume <= value))
                    continue;

                newRoom[nx, ny].Decibels[sourceIndex] = newVolume;
                queue.Enqueue(new Tuple<int, int>(nx, ny));

                _maxDecibels = double.Max(_maxDecibels, newVolume);
                _minDecibels = double.Min(_minDecibels, newVolume);
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
                var normalizedDecibels = (_room[i, j].Decibels.Sum(x => x.Value) - _minDecibels) / decibelRange;
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
    private readonly Queue<float> _buffersLeft = [];
    private readonly Queue<float> _buffersRight = [];

    private double _maxDecibels = 1;
    private double _minDecibels;

    private void ChangeScene(object? sender, RoutedEventArgs _)
    {
        var button = (sender as Button)!;
        var value = int.Parse(button.Name![6].ToString());

        SelectLayout(value);
    }
}

public class Box
{
    public BoxType Type { get; set; } = BoxType.Air;
    public Dictionary<int, double> Decibels { get; } = [];
}

public enum BoxType
{
    Air,
    Wall,
    Source
}