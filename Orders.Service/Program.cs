using Microsoft.AspNetCore.Builder;
using Orders.Service.Clients;
using Orders.Service.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var config = builder.Configuration;
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IConfiguration>(config);
builder.Services.AddSingleton<IChannelEngineClient, ChannelEngineClient>();
builder.Services.AddTransient<IOrderService, OrderService>();

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseHttpsRedirection();
app.UseRouting();
// app.UseAuthorization(); // optional
app.MapControllers();
app.Run();