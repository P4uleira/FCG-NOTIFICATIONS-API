namespace FCG.Notifications.Application.Commands.SendPurchaseConfirmation;

public record PurchaseConfirmationResult(
    PurchaseConfirmationStatus Status,
    bool NotificationSent);