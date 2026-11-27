using System.Drawing;
using Maxim_Technology.Strategies;

namespace Maxim_Technology;

public class UseCaseOrder(Map map, DriverIndex driverIndex, RandomNumber randomNumber)
{
    public async Task<DriverLocationResponse> Find(OrderRequest order)
    {
        //todo: order.Id пригодится для логирования
        var location = new OrderLocation(order.X, order.Y);
        if (!map.Valid(location.X, location.Y)) return new DriverLocationResponse(Reasons: "Координаты некорректны");
        var result = driverIndex.FindNearest(location);

        var driver = result[await randomNumber.Next(result.Count)];

        var route = RouteBuilder.Build(new Point(location.X, location.Y), new Point(driver.X, driver.Y));

        return new DriverLocationResponse(
            new DriverResponse(driver,
                route.Points.Count - 1,
                route.Points.Select(x => new PointRecord(x.X, x.Y)).ToList()));
    }
}

public record OrderRequest(Guid Id, int X, int Y);

public record DriverLocationResponse(DriverResponse? Drivers = null, string? Reasons = null);

public record DriverResponse(Driver Driver, int Distance, List<PointRecord> Route);

public record PointRecord(int X, int Y);

public class Driver(Guid id, int x, int y)
{
    public Guid Id { get; set; } = id;
    public int X { get; set; } = x;
    public int Y { get; set; } = y;
}

public record OrderLocation(int X, int Y);