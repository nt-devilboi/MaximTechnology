namespace MaximTechnology.Entity;

public class Driver(Guid id, int x, int y)
{
    public Guid Id { get; set; } = id;
    public int X { get; set; } = x;
    public int Y { get; set; } = y;
}