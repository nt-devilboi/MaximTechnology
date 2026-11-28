using MaximTechnology.Dto;

namespace MaximTechnology.Web.Extensions;

public static class DriverEndpointsExtensions
{
    public static IEndpointRouteBuilder MapDriverEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/drivers/nearest", async (OrderRequest order, UseCaseOrder useCaseOrder) =>
            {
                var (driver, reasons) = await useCaseOrder.Find(order);

                if (reasons != null) return Results.BadRequest(reasons);

                return driver is null
                    ? Results.NotFound()
                    : Results.Ok(driver);
            })
            .Produces<DriverResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces<string>(StatusCodes.Status400BadRequest);

        endpoints.MapPost("/drivers/location", (DriverUpsert driver, UseCaseDriver useCaseDriver) =>
            {
                var (message, reasons) = useCaseDriver.UpSertDriver(driver);

                return reasons != null ? Results.BadRequest(reasons) : Results.Ok(message);
            })
            .Produces<string>()
            .Produces<string>(StatusCodes.Status400BadRequest);

        return endpoints;
    }
}