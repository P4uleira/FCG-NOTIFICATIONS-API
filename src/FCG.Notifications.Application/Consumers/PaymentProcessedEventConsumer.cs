using FCG.Notifications.Application.Commands.SendPurchaseConfirmation;
using FCG.Payments.Contracts.Events;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FCG.Notifications.Application.Consumers;

public class PaymentProcessedEventConsumer
    : IConsumer<PaymentProcessedEvent>
{
    private readonly ISender _sender;
    private readonly ILogger<PaymentProcessedEventConsumer> _logger;

    public PaymentProcessedEventConsumer(
        ISender sender,
        ILogger<PaymentProcessedEventConsumer> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    public async Task Consume(
        ConsumeContext<PaymentProcessedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            """
            PaymentProcessedEvent recebido.
            OrderId: {OrderId}
            UserId: {UserId}
            GameId: {GameId}
            Price: {Price}
            Status: {Status}
            ProcessedAt: {ProcessedAt}
            """,
            message.OrderId,
            message.UserId,
            message.GameId,
            message.Price,
            message.Status,
            message.ProcessedAt);

        var command = new SendPurchaseConfirmationCommand(
            message.OrderId,
            message.UserId,
            message.GameId,
            message.Price,
            message.Status,
            message.ProcessedAt);

        var result = await _sender.Send(
            command,
            context.CancellationToken);

        _logger.LogInformation(
            """
            PaymentProcessedEvent processado com sucesso.
            OrderId: {OrderId}
            UserId: {UserId}
            GameId: {GameId}
            Price: {Price}
            Status: {PaymentStatus}
            ProcessedAt: {ProcessedAt}
            ProcessingResult: {ProcessingResult}
            NotificationSent: {NotificationSent}
            """,
            message.OrderId,
            message.UserId,
            message.GameId,
            message.Price,
            message.Status,
            message.ProcessedAt,
            result.Status,
            result.NotificationSent);
    }
}