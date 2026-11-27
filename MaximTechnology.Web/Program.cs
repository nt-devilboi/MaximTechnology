using Maxim_Technology;
using MaximTechnology.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDriverMap();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapGet("/drivers/nearest", (int x, int y, UseCaseOrder useCaseOrder) =>
    {
        var (drivers, reasons) = useCaseOrder.Find(new OrderLocation(x, y));

        if (reasons != null) return Results.BadRequest(reasons);

        return drivers is null || drivers.Count == 0
            ? Results.NotFound()
            : Results.Ok(drivers.Select(driver => new DriverResponse(driver.Id, driver.X, driver.Y)).ToList());
    })
    .Produces<List<DriverResponse>>()
    .Produces(StatusCodes.Status404NotFound)
    .Produces<string>(StatusCodes.Status400BadRequest);


app.Run();