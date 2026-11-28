using Maxim_Technology;
using MaximTechnology.Strategies;

namespace MaximTechnology;

public class UseCaseDriver(Map map, DriverIndex driverIndex)
{
    public DriverUpsertResponse UpSertDriver(DriverUpSert driver)
    {
        if (!map.Valid(driver.X, driver.Y))
        {
            driverIndex.Remove(driver.Id);
            return new DriverUpsertResponse(Reasons: "Координаты некорректны");
        }

        var exists = driverIndex.Contains(driver.Id);
        if (!driverIndex.Upsert(new Driver(driver.Id, driver.X, driver.Y)))
            return new DriverUpsertResponse(Reasons: "Здесь уже находится другой водитель");

        return new DriverUpsertResponse(exists ? "Координаты успешно изменены" : "Координаты успешно добавлены");
    }
}

public record DriverUpsertResponse(string? Message = null, string? Reasons = null);

public record DriverUpSert(Guid Id, int X, int Y);