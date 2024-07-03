using Avalonia;

namespace SoundPropagation.Models;

public class Tile
{
    public double Pressure { get; set; } // between -1 and 1
    public Vector Direction { get; set; }
    public bool IsWall { get; set; }
}