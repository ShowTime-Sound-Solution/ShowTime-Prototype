using System;
using System.Collections.Generic;
using Avalonia;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace project;

public partial class MainWindow : Window
{
    public static Client.Client Client = new Client.Client();
    private bool _isFullScreen = false;
    public MainWindow()
    {
        InitializeComponent();
        this.KeyDown += MainWindow_KeyDown;
        
        OnRunSimulation(null, [0]);
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
    
    private void OnRunSimulation(object? sender, byte[] buffer)
    {
        if (buffer.Length < 4)
            return;
        var floats = new float[512];
        for (var i = 0; i < 512; i++)
        {
            floats[i] = BitConverter.ToSingle(buffer, i * 4);
        }
        var decibels = floats.Max(x => Math.Abs(x)) * 300;
        if (decibels > 300)
            return;
        
        /*foreach (var x in buffer)
        {
            floats[x] = BitConverter.ToSingle(buffer, x);
        }*/
        //var decibels = floats.Max(x => Math.Abs(x)) * 1;
        //Console.WriteLine(decibels);
        
        //decibels = Math.Max(0.001f, Math.Min(decibels, 300));
        //Console.WriteLine(decibels);
        InitRoom(_room);
        AddAudioSource(RoomHeight / 2, RoomWidth / 3 * 2, decibels);
        // Add a wall in the room
        for (var i = 0; i < RoomHeight / 2; i++)
            _room[RoomHeight / 4 + i, RoomWidth / 4] = new Box { Type = BoxType.Wall };
        for (var i = 0; i < RoomWidth / 4; i++)
            _room[RoomWidth / 4, RoomHeight / 2 + i] = new Box { Type = BoxType.Wall };
        for (var i = 0; i < RoomWidth / 4; i++)
            _room[RoomWidth / 4 * 3 - 1, RoomHeight / 2 + i] = new Box { Type = BoxType.Wall };
        for (var i = 0; i < RoomHeight / 2; i++)
            _room[RoomHeight / 4 + i, RoomWidth / 4 * 3] = new Box { Type = BoxType.Wall };

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
        var newRoom = (Box[,])_room.Clone();

        foreach (var (x, y) in _audioSources)
            PropagateSoundFromSource(x, y, newRoom);

        _room = newRoom;
    }

    private void PropagateSoundFromSource(int x, int y, Box[,] newRoom)
    {
        var queue = new Queue<Tuple<int, int>>();
        queue.Enqueue(new Tuple<int, int>(x, y));
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            x = current.Item1;
            y = current.Item2;
            var decibels = newRoom[x, y].Decibels;
            if (decibels < 0.01)
                continue;

            var neighbors = GetNeighbors(x, y, 1);
            foreach (var (nx, ny) in neighbors)
            {
                if (newRoom[nx, ny].Type == BoxType.Wall)
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

    private List<Tuple<int, int>> GetNeighbors(int x, int y, int distance)
    {
        var neighbors = new List<Tuple<int, int>>();
        for (var i = -distance; i <= distance; i++)
        for (var j = -distance; j <= distance; j++)
        {
            if (i != distance && j != distance && i != -distance && j != -distance)
                continue;
            if (x + i < 0 || x + i >= RoomWidth || y + j < 0 || y + j >= RoomHeight)
                continue;
            neighbors.Add(new Tuple<int, int>(x + i, y + j));
        }

        return neighbors;
    }

    private void DrawRoom()
    {
        var bitmap = new WriteableBitmap(new PixelSize(RoomSize, RoomSize), new Vector(96, 96), PixelFormat.Bgra8888,
            AlphaFormat.Premul);
        var pixels = new byte[RoomSize * RoomSize * 4];

        for (var i = 0; i < RoomWidth; i++)
        {
            for (var j = 0; j < RoomHeight; j++)
            {
                var color = -1;
                if (_room[i, j].Type == BoxType.Air)
                {
                    var normalizedDecibels = (_room[i, j].Decibels - _minDecibels) / (_maxDecibels - _minDecibels);
                    normalizedDecibels = Math.Max(0, Math.Min(normalizedDecibels, 1));
                    color = (int)(normalizedDecibels * 255);
                }

                for (var k = 0; k < PixelSize; k++)
                for (var l = 0; l < PixelSize; l++)
                {
                    var index = (i * PixelSize + k) * RoomSize * 4 + (j * PixelSize + l) * 4;
                    pixels[index + 3] = 255;

                    switch (_room[i, j].Type)
                    {
                        case BoxType.Wall:
                            pixels[index] = 0;
                            pixels[index + 1] = 0;
                            pixels[index + 2] = 0;
                            break;
                        case BoxType.Source:
                            pixels[index] = 0;
                            pixels[index + 1] = 255;
                            pixels[index + 2] = 0;
                            break;
                        case BoxType.Air:
                        default:
                            pixels[index] = (byte)(255 - color);
                            pixels[index + 1] = 0;
                            pixels[index + 2] = (byte)color;
                            break;
                    }
                }
            }
        }

        using (var bitmapLock = bitmap.Lock())
        {
            var ptr = bitmapLock.Address;
            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, ptr, pixels.Length);
        }

        this.Find<Image>("SimulationImage")!.Source = bitmap;
    }

    private void InitRoom(Box[,] room)
    {
        for (var i = 0; i < RoomWidth; i++)
        for (var j = 0; j < RoomHeight; j++)
            room[i, j] = new Box();
    }

    private const int RoomWidth = 240;
    private const int RoomHeight = 240;
    private const int RoomSize = 721;
    private const int PixelSize = RoomSize / RoomWidth;

    private Box[,] _room = new Box[RoomHeight, RoomWidth];
    private readonly List<Tuple<int, int>> _audioSources = [];

    private double _maxDecibels = 1;
    private double _minDecibels;
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