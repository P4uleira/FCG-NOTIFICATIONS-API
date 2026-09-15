using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace FCG.Notifications.WelcomeLambda;

public class Function
{
    public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
    {
        foreach (var message in evnt.Records)
        {
            await ProcessMessageAsync(message, context);
        }
    }

    private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
    {
        var userCreated = JsonSerializer.Deserialize<UserCreatedMessage>(message.Body);

        if (userCreated is null)
        {
            context.Logger.LogWarning($"Nao foi possivel desserializar a mensagem: {message.Body}");
            return;
        }

        context.Logger.LogInformation(
            "=========================================\n" +
            "E-MAIL DE BOAS-VINDAS SIMULADO\n" +
            $"UserId: {userCreated.UserId}\n" +
            $"Name: {userCreated.Name}\n" +
            $"Email: {userCreated.Email}\n" +
            $"Role: {userCreated.Role}\n" +
            $"CreatedAt: {userCreated.CreatedAt}\n" +
            $"Mensagem: Bem-vindo a FIAP Cloud Games, {userCreated.Name}!\n" +
            "=========================================");

        await Task.CompletedTask;
    }
}

public sealed record UserCreatedMessage(
    Guid UserId,
    string Name,
    string Email,
    string Role,
    DateTime CreatedAt);