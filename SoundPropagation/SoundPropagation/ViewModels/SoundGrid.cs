using System;
using System.Collections.Generic;
using System.Linq;
using SoundPropagation.Models;
using Vector = Avalonia.Vector;

namespace SoundPropagation.ViewModels;

public class SoundGrid
{
    public SoundGrid(int width, int height)
    {
        Width = width;
        Height = height;
        _room = new Tile[width, height];
        _newRoom = new Tile[width, height];

        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)
        {
            _room[x, y] = new Tile();
            _newRoom[x, y] = new Tile();
        }
    }

    public Tile this[int x, int y]
    {
        get => _room[x, y];
        set => _room[x, y] = value;
    }

    public void AddAudioSource(int x, int y, double pressure = 1, Vector direction = default)
    {
        _room[x, y].Pressure = pressure;
        _room[x, y].Direction = direction;
    }

    public void AddVerticalWall(int x, int y, int height)
    {
        for (var i = 0; i < height; i++)
            _room[x, y + i].IsWall = true;
    }

    public void AddHorizontalWall(int x, int y, int width)
    {
        for (var i = 0; i < width; i++)
            _room[x + i, y].IsWall = true;
    }

    public void Simulate()
    {
        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Height; y++)
        {
            _newRoom[x, y] = new Tile
            {
                Pressure = _room[x, y].Pressure,
                Direction = _room[x, y].Direction,
                IsWall = _room[x, y].IsWall,
            };
        }

        PropagateSound();
        HandleBoundaries();

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Height; y++)
        {
            _room[x, y] = new Tile
            {
                Pressure = _newRoom[x, y].Pressure,
                Direction = _newRoom[x, y].Direction,
                IsWall = _newRoom[x, y].IsWall,
            };
        }
    }

    private void PropagateSound()
    {
        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Height; y++)
        {
            if (_room[x, y].IsWall) continue;
            UpdatePressure(x, y);
        }
    }

    private void UpdatePressure(int x, int y)
    {
        var totalPressure = 0.0;
        var totalWeight = 0.0;
        var totalDirection = new Vector(0, 0);

        foreach (var (nx, ny) in GetNeighbors(x, y))
        {
            if (_room[nx, ny].IsWall) continue;

            // add pressure from the neighbor based on the direction of the sound
            var direction = _room[nx, ny].Direction.Normalize();
            var neighborVector = new Vector(nx - x, ny - y).Normalize();
            var alignment = (1 + Vector.Dot(direction, neighborVector)) / 2;

            if (double.IsNaN(alignment)) continue;
            totalPressure += _room[nx, ny].Pressure * alignment;
            totalWeight += alignment;

            // update direction
            totalDirection += direction * _room[nx, ny].Pressure;
        }

        if (totalWeight != 0)
            _newRoom[x, y].Pressure = totalPressure / totalWeight * 0.99; // Attenuation factor
        else
            _newRoom[x, y].Pressure = _room[x, y].Pressure * 0.99;
        _newRoom[x, y].Direction = totalDirection.Normalize();
    }

    private void HandleBoundaries()
    {
        const double absorptionFactor = 0.9; // Adjust this factor for how much sound is absorbed by walls

        for (var x = 0; x < Width; x++)
        {
            for (var y = 0; y < Height; y++)
            {
                if (_newRoom[x, y].IsWall)
                {
                    // Reflect the sound by reversing the direction perpendicular to the wall
                    // Assuming walls are vertical or horizontal, we simplify reflection handling

                    // Vertical walls
                    if (x > 0 && !_newRoom[x - 1, y].IsWall)
                    {
                        _newRoom[x - 1, y].Direction = new Vector(-_newRoom[x - 1, y].Direction.X,
                            _newRoom[x - 1, y].Direction.Y);
                        _newRoom[x - 1, y].Pressure *= absorptionFactor;
                    }

                    if (x < Width - 1 && !_newRoom[x + 1, y].IsWall)
                    {
                        _newRoom[x + 1, y].Direction = new Vector(-_newRoom[x + 1, y].Direction.X,
                            _newRoom[x + 1, y].Direction.Y);
                        _newRoom[x + 1, y].Pressure *= absorptionFactor;
                    }

                    // Horizontal walls
                    if (y > 0 && !_newRoom[x, y - 1].IsWall)
                    {
                        _newRoom[x, y - 1].Direction = new Vector(_newRoom[x, y - 1].Direction.X,
                            -_newRoom[x, y - 1].Direction.Y);
                        _newRoom[x, y - 1].Pressure *= absorptionFactor;
                    }

                    if (y < Height - 1 && !_newRoom[x, y + 1].IsWall)
                    {
                        _newRoom[x, y + 1].Direction = new Vector(_newRoom[x, y + 1].Direction.X,
                            -_newRoom[x, y + 1].Direction.Y);
                        _newRoom[x, y + 1].Pressure *= absorptionFactor;
                    }
                }
            }
        }
    }


    private IEnumerable<Tuple<int, int>> GetNeighbors(int x, int y)
    {
        return _neighbors.Select(pair => new Tuple<int, int>(pair.Item1 + x, pair.Item2 + y)).Where(pair =>
            pair.Item1 >= 0 && pair.Item1 < Width && pair.Item2 >= 0 && pair.Item2 < Height);
    }

    private readonly Tile[,] _room;
    private readonly Tile[,] _newRoom;

    private readonly List<Tuple<int, int>> _neighbors =
    [
        new Tuple<int, int>(-1, -1),
        new Tuple<int, int>(-1, 0),
        new Tuple<int, int>(-1, 1),
        new Tuple<int, int>(0, -1),
        new Tuple<int, int>(0, 1),
        new Tuple<int, int>(1, -1),
        new Tuple<int, int>(1, 0),
        new Tuple<int, int>(1, 1)
    ];

    private int Width { get; }
    private int Height { get; }
}