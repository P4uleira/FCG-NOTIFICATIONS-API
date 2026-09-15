using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace FCG.Notifications.PurchaseConfirmationLambda;

public class Function
{
    private const string ApprovedStatus = "Approved";

    public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
    {
        foreach (var message in evnt.Records)
        {
            await ProcessMessageAsync(message, context);
        }
    }

    private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
    {
        var paymentProcessed = JsonSerializer.Deserialize<PaymentProcessedMessage>(message.Body);

        if (paymentProcessed is null)
        {
            context.Logger.LogWarning($"Nao foi possivel desserializar a mensagem: {message.Body}");
            return;
        }

        if (!string.Equals(paymentProcessed.Status, ApprovedStatus, StringComparison.OrdinalIgnoreCase))
        {
            context.Logger.LogInformation(
                "Pagamento rejeitado.\n" +
                $"OrderId: {paymentProcessed.OrderId}\n" +
                $"UserId: {paymentProcessed.UserId}\n" +
                $"GameId: {paymentProcessed.GameId}\n" +
                $"Price: {paymentProcessed.Price}\n" +
                $"Status: {paymentProcessed.Status}");

            context.Logger.LogInformation(
                $"Nenhuma notificacao enviada.\nOrderId: {paymentProcessed.OrderId}");

            return;
        }

        context.Logger.LogInformation(
            "Pagamento aprovado.\n" +
            $"OrderId: {paymentProcessed.OrderId}\n" +
            $"UserId: {paymentProcessed.UserId}\n" +
            $"GameId: {paymentProcessed.GameId}\n" +
            $"Price: {paymentProcessed.Price}\n" +
            $"Status: {paymentProcessed.Status}");

        context.Logger.LogInformation(
            "=========================================\n" +
            "E-MAIL DE CONFIRMACAO DE COMPRA SIMULADO\n" +
            $"OrderId: {paymentProcessed.OrderId}\n" +
            $"UserId: {paymentProcessed.UserId}\n" +
            $"GameId: {paymentProcessed.GameId}\n" +
            $"Price: {paymentProcessed.Price}\n" +
            $"Status: {paymentProcessed.Status}\n" +
            $"ProcessedAt: {paymentProcessed.ProcessedAt}\n" +
            "=========================================");

        context.Logger.LogInformation(
            $"E-mail de confirmacao de compra simulado para o usuario {paymentProcessed.UserId}.");

        await Task.CompletedTask;
    }
}

public sealed record PaymentProcessedMessage(
    Guid OrderId,
    Guid UserId,
    Guid GameId,
    decimal Price,
    string Status,
    DateTime ProcessedAt);