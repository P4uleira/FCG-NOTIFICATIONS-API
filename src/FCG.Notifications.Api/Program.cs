using FCG.Notifications.Application.Consumers;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<UserCreatedEventConsumer>();

    config.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqConfig = builder.Configuration.GetSection("RabbitMQ");

        cfg.Host(
            rabbitMqConfig["Host"],
            ushort.Parse(rabbitMqConfig["Port"] ?? "5672"),
            "/",
            h =>
            {
                h.Username(rabbitMqConfig["Username"] ?? "guest");
                h.Password(rabbitMqConfig["Password"] ?? "guest");
            });

        cfg.ReceiveEndpoint("notifications-user-created-event", endpoint =>
        {
            endpoint.ConfigureConsumer<UserCreatedEventConsumer>(context);
        });
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => "FCG Notifications API is running.");

app.Run();