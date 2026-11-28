using Maxim_Technology;
using MaximTechnology;
using MaximTechnology.Web;
using MaximTechnology.Web.Extensions;

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
    .Produces<DriverResponse>()
    .Produces(StatusCodes.Status404NotFound)
    .Produces<string>(StatusCodes.Status400BadRequest);


app.MapPost("/drivers/location", (DriverUpSert driver, UseCaseDriver useCaseDriver) =>
    {
        var (message, reasons) = useCaseDriver.UpSertDriver(driver);

        return reasons != null ? Results.BadRequest(reasons) : Results.Ok(message);
    })
    .Produces<string>()
    .Produces<string>(StatusCodes.Status400BadRequest);


var limit = int.Parse(builder.Configuration.GetSection("Settings")["ParallelLimit"]);
if (limit <= 0) throw new ArgumentException();

app.UseMiddleware<ParallelLimitMiddleware>(limit);

app.Run();