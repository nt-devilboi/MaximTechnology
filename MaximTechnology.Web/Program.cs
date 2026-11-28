using MaximTechnology.Web;
using MaximTechnology.Web.Extensions;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ILog>(new ConsoleLog());
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
app.MapDriverEndpoints();

var limit = int.Parse(builder.Configuration.GetSection("Settings")["ParallelLimit"]);
if (limit <= 0) throw new ArgumentException();
app.UseMiddleware<ParallelLimitMiddleware>(limit);

app.Run();