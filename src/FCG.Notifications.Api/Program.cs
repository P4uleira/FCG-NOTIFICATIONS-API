using FCG.Notifications.Application.Commands.SendPurchaseConfirmation;
using FCG.Notifications.Application.Consumers;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

#region Controllers

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#endregion

#region MediatR

builder.Services.AddMediatR(configuration =>
{
    configuration.RegisterServicesFromAssembly(
        typeof(SendPurchaseConfirmationCommand).Assembly);
});

#endregion

#region MassTransit

builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<UserCreatedEventConsumer>();
    config.AddConsumer<PaymentProcessedEventConsumer>();

    config.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqConfig =
            builder.Configuration.GetSection("RabbitMq");

        cfg.Host(
            rabbitMqConfig["Host"],
            "/",
            host =>
            {
                host.Username(rabbitMqConfig["Username"]!);
                host.Password(rabbitMqConfig["Password"]!);
            });

        cfg.ReceiveEndpoint(
            "notifications-user-created-event",
            endpoint =>
            {
                endpoint.ConfigureConsumer<
                    UserCreatedEventConsumer>(context);
            });

        cfg.ReceiveEndpoint(
            "notifications-payment-processed-event",
            endpoint =>
            {
                endpoint.ConfigureConsumer<
                    PaymentProcessedEventConsumer>(context);
            });
    });
});

#endregion

var app = builder.Build();

#region Pipeline

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () =>
    Results.Ok(new
    {
        service = "NotificationsAPI",
        message = "FCG Notifications API is running."
    }));

app.MapGet("/health", () =>
    Results.Ok(new
    {
        service = "NotificationsAPI",
        status = "Healthy"
    }));

#endregion

app.Run();