using MediatR;
using Microsoft.Extensions.Logging;

namespace FCG.Notifications.Application.Commands.SendPurchaseConfirmation;

public class SendPurchaseConfirmationCommandHandler
    : IRequestHandler<
        SendPurchaseConfirmationCommand,
        PurchaseConfirmationResult>
{
    private const string ApprovedStatus = "Approved";

    private readonly ILogger<SendPurchaseConfirmationCommandHandler> _logger;

    public SendPurchaseConfirmationCommandHandler(
        ILogger<SendPurchaseConfirmationCommandHandler> logger)
    {
        _logger = logger;
    }

    public Task<PurchaseConfirmationResult> Handle(
        SendPurchaseConfirmationCommand request,
        CancellationToken cancellationToken)
    {
        var paymentApproved = string.Equals(
            request.Status,
            ApprovedStatus,
            StringComparison.OrdinalIgnoreCase);

        if (paymentApproved)
        {
            ProcessApprovedPayment(request);

            return Task.FromResult(
                new PurchaseConfirmationResult(
                    PurchaseConfirmationStatus.NotificationSent,
                    NotificationSent: true));
        }

        ProcessRejectedPayment(request);

        return Task.FromResult(
            new PurchaseConfirmationResult(
                PurchaseConfirmationStatus.PaymentRejected,
                NotificationSent: false));
    }

    private void ProcessApprovedPayment(
        SendPurchaseConfirmationCommand request)
    {
        _logger.LogInformation(
            """
            Pagamento aprovado.
            OrderId: {OrderId}
            UserId: {UserId}
            GameId: {GameId}
            Price: {Price}
            Status: {Status}
            ProcessedAt: {ProcessedAt}
            """,
            request.OrderId,
            request.UserId,
            request.GameId,
            request.Price,
            request.Status,
            request.ProcessedAt);

        _logger.LogInformation(
            """
            =========================================
            E-MAIL DE CONFIRMAÇÃO DE COMPRA SIMULADO
            OrderId: {OrderId}
            UserId: {UserId}
            GameId: {GameId}
            Price: {Price}
            Status: {Status}
            ProcessedAt: {ProcessedAt}
            =========================================
            """,
            request.OrderId,
            request.UserId,
            request.GameId,
            request.Price,
            request.Status,
            request.ProcessedAt);

        _logger.LogInformation(
            "E-mail de confirmação de compra simulado para o usuário {UserId}, referente ao pedido {OrderId}.",
            request.UserId,
            request.OrderId);
    }

    private void ProcessRejectedPayment(
        SendPurchaseConfirmationCommand request)
    {
        _logger.LogWarning(
            """
            Pagamento rejeitado.
            OrderId: {OrderId}
            UserId: {UserId}
            GameId: {GameId}
            Price: {Price}
            Status: {Status}
            ProcessedAt: {ProcessedAt}
            """,
            request.OrderId,
            request.UserId,
            request.GameId,
            request.Price,
            request.Status,
            request.ProcessedAt);

        _logger.LogInformation(
            """
            Nenhuma notificação enviada.
            OrderId: {OrderId}
            UserId: {UserId}
            GameId: {GameId}
            Price: {Price}
            Status: {Status}
            ProcessedAt: {ProcessedAt}
            """,
            request.OrderId,
            request.UserId,
            request.GameId,
            request.Price,
            request.Status,
            request.ProcessedAt);
    }
}