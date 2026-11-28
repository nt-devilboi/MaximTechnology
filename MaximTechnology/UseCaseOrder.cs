using System.Drawing;
using MaximTechnology.Dto;
using MaximTechnology.Strategies;
using Vostok.Logging.Abstractions;

namespace MaximTechnology;

public class UseCaseOrder(Map map, DriverIndex driverIndex, RandomNumber randomNumber, ILog log)
{
    private readonly ILog log = log.ForContext(nameof(UseCaseOrder));

    public async Task<DriverLocationResult> Find(OrderRequest order)
    {
        try
        {
            var location = new OrderLocation(order.X, order.Y);
            if (!map.Valid(location.X, location.Y))
            {
                log.Warn("Координаты заказа вне карты: {Order}", order);
                return new DriverLocationResult(Reasons: "Координаты некорректны");
            }

            var drivers = driverIndex.FindNearest(location);

            if (drivers.Count == 0) return new DriverLocationResult();

            var driver = drivers[await randomNumber.Next(drivers.Count)];


            var route = RouteBuilder.Build(new Point(location.X, location.Y), new Point(driver.X, driver.Y));

            return new DriverLocationResult(
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

public record OrderLocation(int X, int Y);