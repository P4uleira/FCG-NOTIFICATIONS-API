using MediatR;

namespace FCG.Notifications.Application.Commands.SendPurchaseConfirmation;

public record SendPurchaseConfirmationCommand(
    Guid OrderId,
    Guid UserId,
    Guid GameId,
    decimal Price,
    string Status,
    DateTime ProcessedAt)
    : IRequest<PurchaseConfirmationResult>;