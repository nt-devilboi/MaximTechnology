using MaximTechnology.Dto;
using MaximTechnology.Entity;
using MaximTechnology.Strategies;
using Vostok.Logging.Abstractions;

namespace MaximTechnology;

public class UseCaseDriver(Map map, DriverIndex driverIndex, ILog log)
{
    private readonly ILog log = log.ForContext(nameof(UseCaseDriver));

    public DriverUpsertResponse UpSertDriver(DriverUpsert driver)
    {
        try
        {
            if (!map.Valid(driver.X, driver.Y))
            {
                driverIndex.Remove(driver.Id);
                log.Warn("Координаты водителя вне карты, водитель убран с карты: {Driver}", driver);
                return new DriverUpsertResponse(Reasons: "Координаты некорректны");
            }

            var state = driverIndex.Upsert(new Driver(driver.Id, driver.X, driver.Y));

            if (state == State.Block)
            {
                log.Warn("Клетка занята другим водителем: {Driver}", driver);
                return new DriverUpsertResponse(Reasons: "Здесь уже находится другой водитель");
            }

            return new DriverUpsertResponse(state == State.Updated
                ? "Координаты успешно изменены"
                : "Координаты успешно добавлены");
        }
        catch (Exception e)
        {
            log.Error(e, "Не удалось обновить координаты водителя {Driver}", driver);
            throw;
        }
    }
}