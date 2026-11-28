using MaximTechnology.Entity;
using MaximTechnology.Strategies;
using Microsoft.OpenApi.Models;

namespace MaximTechnology.Web.Extensions;

public static class DriverMapExtensions
{
    public static IServiceCollection AddDriverMap(this IServiceCollection services,
        ConfigurationManager configurationManager)
    {
        var settingsX = configurationManager.GetSection("MapSettings")["X"];
        var settingsY = configurationManager.GetSection("MapSettings")["Y"];

        if (settingsX == null || !int.TryParse(settingsX, out var x) || settingsY == null ||
            !int.TryParse(settingsY, out var y))
            throw new ApplicationException("not correct MapSettings");

        var map = new Map { X = x, Y = y };
        var driverIndex = new RingDriverIndex(map);

        var drivers = new Driver[]
        {
            new(Guid.NewGuid(), 1, 1),
            new(Guid.NewGuid(), 8, 2),
            new(Guid.NewGuid(), 4, 5),
            new(Guid.NewGuid(), 2, 8),
            new(Guid.NewGuid(), 7, 7)
        };
        foreach (var driver in drivers) driverIndex.Upsert(driver);

        services.AddSingleton(map);
        services.AddSingleton<DriverIndex>(driverIndex);
        services.AddHttpClient<RandomNumber>();
        services.AddScoped<UseCaseOrder>();
        services.AddScoped<UseCaseDriver>();

        // Id генерируются при каждом запуске, поэтому показываем их в шапке Swagger UI, чтобы было что подставить в запрос
        services.ConfigureSwaggerGen(options => options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "MaximTechnology",
            Version = "v1",
            Description = $"размер карты ({map.X}, {map.Y}) \n Водители, созданные при запуске:\n\n" +
                          string.Join("\n", drivers.Select(d => $"- `{d.Id}` — ({d.X}, {d.Y})"))
        }));

        return services;
    }
}