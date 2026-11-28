using System.Drawing;
using MaximTechnology.Strategies;
using Vostok.Logging.Abstractions;

namespace MaximTechnology;

public class UseCaseOrder(Map map, DriverIndex driverIndex, RandomNumber randomNumber, ILog log)
{
    private readonly ILog log = log.ForContext(nameof(UseCaseOrder));

    public async Task<DriverLocationResponse> Find(OrderRequest order)
    {
        try
        {
            var location = new OrderLocation(order.X, order.Y);
            if (!map.Valid(location.X, location.Y))
            {
                log.Warn("Координаты заказа вне карты: {Order}", order);
                return new DriverLocationResponse(Reasons: "Координаты некорректны");
            }

            var drivers = driverIndex.FindNearest(location);

            if (drivers.Count == 0) return new DriverLocationResponse();

            var driver = drivers[await randomNumber.Next(drivers.Count)];


            var route = RouteBuilder.Build(new Point(location.X, location.Y), new Point(driver.X, driver.Y));

            return new DriverLocationResponse(
                new DriverResponse(driver,
                    route.Points.Count - 1,
                    route.Points.Select(x => new PointRecord(x.X, x.Y)).ToList()));
        }
        catch (Exception e)
        {
            log.Error(e, "Не удалось подобрать водителя для заказа {Order}", order);
            throw;
        }
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