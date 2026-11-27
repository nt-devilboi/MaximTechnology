using Maxim_Technology;
using MaximTechnology.Web.Extensions;
using DriverResponse = MaximTechnology.Web.Dto.DriverResponse;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDriverMap(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapPost("/drivers/nearest", async (OrderRequest order, UseCaseOrder useCaseOrder) =>
    {
        var (driver, reasons) = await useCaseOrder.Find(order);

        if (reasons != null) return Results.BadRequest(reasons);

        return driver is null
            ? Results.NotFound()
            : Results.Ok(driver);
    })
    .Produces<Maxim_Technology.DriverResponse>()
    .Produces(StatusCodes.Status404NotFound)
    .Produces<string>(StatusCodes.Status400BadRequest);


app.MapPost("/drivers/location", (DriverUpSert driver, UseCaseDriver useCaseDriver) =>
    {
        var (message, reasons) = useCaseDriver.UpSertDriver(driver);

        return reasons != null ? Results.BadRequest(reasons) : Results.Ok(message);
    })
    .Produces<string>()
    .Produces<string>(StatusCodes.Status400BadRequest);


app.Run();