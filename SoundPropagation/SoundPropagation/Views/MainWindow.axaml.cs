using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using SoundPropagation.ViewModels;

namespace SoundPropagation.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        DispatcherTimer.Run(() =>
        {
            OnRunSimulation(null, null);
            return true;
        }, TimeSpan.FromMilliseconds(1));

        _room.AddAudioSource(RoomWidth / 3 * 2, RoomHeight / 2, 1, new Vector(1, 0));

        // Add walls in the room
        _room.AddVerticalWall(RoomWidth / 4, RoomHeight / 4, RoomHeight / 2);
        _room.AddHorizontalWall(RoomWidth / 2, RoomHeight / 4, RoomHeight / 4);
        _room.AddHorizontalWall(RoomWidth / 2, RoomHeight / 4 * 3 - 1, RoomHeight / 4);
        _room.AddVerticalWall(RoomWidth / 4 * 3, RoomHeight / 4, RoomHeight / 2);

        DrawRoom();
    }

    private void OnRunSimulation(object? sender, RoutedEventArgs? e)
    {
        var decibels = new Random().NextDouble() * 100 + 75;

        _room.Simulate();
        DrawRoom();
    }

    private void DrawRoom()
    {
        var bitmap = new WriteableBitmap(new PixelSize(RoomSize, RoomSize), new Vector(96, 96), PixelFormat.Rgba8888,
            AlphaFormat.Premul);
        var pixels = new byte[RoomSize * RoomSize * 4];

        for (var i = 0; i < RoomWidth; i++)
        for (var j = 0; j < RoomHeight; j++)
        {
            for (var x = 0; x < PixelSize; x++)
            for (var y = 0; y < PixelSize; y++)
            {
                var index = ((j * PixelSize + y) * RoomSize + i * PixelSize + x) * 4;

                pixels[index + 3] = 255;
                if (_room[i, j].IsWall)
                {
                    pixels[index] = 0;
                    pixels[index + 1] = 0;
                    pixels[index + 2] = 0;
                    continue;
                }

                // gradient from blue (-1) to red (1)
                var pressure = _room[i, j].Pressure;
                var color = new Vector3D(1, 1 - pressure * 2, 1 - pressure * 2);

                pixels[index] = (byte)Math.Max(0, Math.Min(color.X * 255, 255));
                pixels[index + 1] = (byte)Math.Max(0, Math.Min(color.Y * 255, 255));
                pixels[index + 2] = (byte)Math.Max(0, Math.Min(color.Z * 255, 255));
            }
        }

        using (var bitmapLock = bitmap.Lock())
        {
            var ptr = bitmapLock.Address;
            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, ptr, pixels.Length);
        }

        SimulationImage.Source = bitmap;
    }

    private const int RoomWidth = 200;
    private const int RoomHeight = 200;
    private const int RoomSize = 600;
    private const int PixelSize = RoomSize / RoomWidth;

    private readonly SoundGrid _room = new(RoomWidth, RoomHeight);
}