using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace SoundPropagation.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public override void BeginInit()
    {
        InitRoom(_room);
        AddAudioSource(RoomWidth / 2, RoomHeight / 2, 60);
        // Add a wall in the room
        for (var i = 0; i < RoomWidth / 2; i++)
            _room[RoomWidth / 4 + i, RoomWidth / 4] = new Box { Type = BoxType.Wall };


        // for (var i = 0; i < RoomWidth; i++)
        // {
        //     for (var j = 0; j < RoomHeight; j++)
        //         Console.Write(_room[i, j]);
        //     Console.WriteLine();
        // }
        base.BeginInit();
    }

    private void OnRunSimulation(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
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
                if (newVolume < 0 || !(newVolume > newRoom[nx, ny].Decibels))
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
                int color;
                if (_room[i, j].Type == BoxType.Wall)
                    color = -1;
                else
                {
                    var normalizedDecibels = (_room[i, j].Decibels - _minDecibels) / (_maxDecibels - _minDecibels);
                    normalizedDecibels = Math.Max(0, Math.Min(normalizedDecibels, 1));
                    color = (int)(normalizedDecibels * 255);
                }

                for (var k = 0; k < PixelSize; k++)
                {
                    for (var l = 0; l < PixelSize; l++)
                    {
                        var index = (i * PixelSize + k) * RoomSize * 4 + (j * PixelSize + l) * 4;
                        pixels[index + 3] = 255;

                        if (color == -1)
                        {
                            pixels[index] = (byte)(_room[i, j].Type == BoxType.Source ? 255 : 0);
                            pixels[index + 1] = 0;
                            pixels[index + 2] = (byte)(_room[i, j].Type == BoxType.Wall ? 255 : 0);
                        }
                        else
                        {
                            pixels[index] = (byte)color;
                            pixels[index + 1] = (byte)color;
                            pixels[index + 2] = (byte)color;
                        }
                    }
                }
            }
        }

        using (var bitmapLock = bitmap.Lock())
        {
            var ptr = bitmapLock.Address;
            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, ptr, pixels.Length);
        }

        SimulationImage.Source = bitmap;
    }

    private void InitRoom(Box[,] room)
    {
        for (var i = 0; i < RoomWidth; i++)
        for (var j = 0; j < RoomHeight; j++)
            room[i, j] = new Box();
    }

    private const int RoomWidth = 100;
    private const int RoomHeight = 100;
    private const int RoomSize = 600;
    private const int PixelSize = RoomSize / RoomWidth;

    private Box[,] _room = new Box[RoomHeight, RoomWidth];
    private readonly List<Tuple<int, int>> _audioSources = new();

    private double _maxDecibels = 1;
    private double _minDecibels = 0;
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