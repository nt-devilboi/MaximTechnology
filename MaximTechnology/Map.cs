namespace MaximTechnology;

public class Map
{
    public int X { get; set; } = 10;
    public int Y { get; set; } = 10;

    public bool Valid(int x, int y)
    {
        return x >= 0 && x < X &&
               y >= 0 && y < Y;
    }
}