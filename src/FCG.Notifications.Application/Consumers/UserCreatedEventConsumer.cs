using FCG.Users.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FCG.Notifications.Application.Consumers;

public sealed class UserCreatedEventConsumer : IConsumer<UserCreatedEvent>
{
    private readonly ILogger<UserCreatedEventConsumer> _logger;

    public UserCreatedEventConsumer(ILogger<UserCreatedEventConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<UserCreatedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            """
            =========================================
            E-MAIL DE BOAS-VINDAS SIMULADO
            UserId: {UserId}
            Name: {Name}
            Email: {Email}
            Role: {Role}
            CreatedAt: {CreatedAt}
            Mensagem: Bem-vindo à FIAP Cloud Games, {Name}!
            =========================================
            """,
            message.UserId,
            message.Name,
            message.Email,
            message.Role,
            message.CreatedAt,
            message.Name
        );

        return Task.CompletedTask;
    }
}