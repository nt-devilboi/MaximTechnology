using Maxim_Technology;
using Maxim_Technology.Strategies;

namespace MaximTechnology.Web.Extensions;

public static class DriverMapExtensions
{
    public static IServiceCollection AddDriverMap(this IServiceCollection services)
    {
        var map = new Map { X = 10, Y = 10 };
        var driverIndex = new RingDriverIndex(map);

        var drivers = new Driver[]
        {
            new(Guid.NewGuid(), 1, 1),
            new(Guid.NewGuid(), 8, 2),
            new(Guid.NewGuid(), 4, 5),
            new(Guid.NewGuid(), 2, 8),
            new(Guid.NewGuid(), 7, 7),
        };
        foreach (var driver in drivers) driverIndex.Upsert(driver);

        services.AddSingleton(map);
        services.AddSingleton<DriverIndex>(driverIndex);
        services.AddScoped<UseCaseOrder>();
        return services;
    }
}