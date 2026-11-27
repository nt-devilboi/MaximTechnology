namespace Maxim_Technology;

public class UseCaseOrder(Map map, DriverIndex driverIndex)
{
    public DriverLocationResponse Find(OrderLocation location)
    {
        if (!map.Valid(location.X, location.Y)) return new DriverLocationResponse(Reasons: "Координаты некорректны");
        var result = driverIndex.FindNearest(location);


        return new DriverLocationResponse(result);
    }
}

public record DriverLocationResponse(List<Driver>? Drivers = null, string? Reasons = null);

public class Driver(Guid id, int x, int y)
{
    public Guid Id { get; set; } = id;
    public int X { get; set; } = x;
    public int Y { get; set; } = y;
}

public record OrderLocation(int X, int Y);