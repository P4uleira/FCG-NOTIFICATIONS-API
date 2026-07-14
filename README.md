# FCG Notifications API

## Visão Geral

A **FCG Notifications API** é o microsserviço responsável pelo processamento das notificações da plataforma **FIAP Cloud Games**.

Seu objetivo é consumir eventos publicados pelos demais microsserviços e simular o envio de notificações aos usuários através de registros em log, desacoplando completamente essa responsabilidade das demais APIs da solução.

No cenário atual do projeto, a NotificationsAPI realiza dois processos principais:

* envio simulado do e-mail de boas-vindas após o cadastro de um usuário;
* envio simulado da confirmação de compra após a aprovação de um pagamento.

Todo o processamento ocorre de forma assíncrona utilizando **RabbitMQ** e **MassTransit**, não sendo necessário qualquer chamada HTTP para iniciar as notificações.

A API possui apenas endpoints auxiliares para verificação da disponibilidade do serviço, não oferecendo endpoints de negócio para utilização pelos clientes da plataforma.

---

# Arquitetura

A NotificationsAPI foi desenvolvida seguindo os princípios de **Domain-Driven Design (DDD)**, **Clean Architecture** e **CQRS**, mantendo a separação entre infraestrutura, casos de uso e integração com os demais microsserviços.

As principais tecnologias utilizadas são:

* .NET 10
* ASP.NET Core Web API
* MediatR
* RabbitMQ
* MassTransit
* Swagger / OpenAPI
* Docker
* Kubernetes
* ILogger

Diferentemente dos demais microsserviços da solução, a NotificationsAPI:

* não possui banco de dados;
* não utiliza Entity Framework Core;
* não realiza autenticação JWT;
* não recebe requisições de negócio diretamente dos clientes.

Sua responsabilidade é exclusivamente consumir eventos publicados por outros microsserviços e executar os casos de uso relacionados às notificações.

---

# Estrutura da Solução

O projeto está organizado em camadas, separando claramente a API, os casos de uso, os consumidores de eventos e os contratos compartilhados.

```text
FCG-Notifications-Api
│
├── src
│   ├── FCG.Notifications.Api
│   │   ├── Program.cs
│   │   ├── Controllers
│   │   ├── appsettings.json
│   │   └── Dockerfile
│   │
│   ├── FCG.Notifications.Application
│   │   ├── Commands
│   │   │   └── SendPurchaseConfirmation
│   │   ├── Consumers
│   │   │   ├── UserCreatedEventConsumer
│   │   │   └── PaymentProcessedEventConsumer
│   │   └── Handlers
│   │
│   └── FCG.Notifications.Contracts
│
└── tests
```

## Camadas

### FCG.Notifications.Api

Responsável por:

* inicialização da aplicação;
* configuração do pipeline HTTP;
* configuração do RabbitMQ;
* configuração do MassTransit;
* Swagger;
* Health Check;
* registro dos consumidores.

---

### FCG.Notifications.Application

Contém toda a lógica da aplicação.

É composta por:

* Commands;
* Handlers;
* Consumers;
* objetos de retorno das notificações.

Essa camada é responsável por interpretar os eventos recebidos e decidir qual tipo de notificação deverá ser executada.

---

### Consumers

Os Consumers representam o ponto de entrada da aplicação.

Atualmente existem dois consumidores:

* **UserCreatedEventConsumer**
* **PaymentProcessedEventConsumer**

Cada Consumer recebe um evento do RabbitMQ e encaminha o processamento para o respectivo caso de uso.

---

### Contracts

Contém os contratos compartilhados utilizados na comunicação entre os microsserviços.

Atualmente a NotificationsAPI consome:

* `UserCreatedEvent`
* `PaymentProcessedEvent`

Esses contratos garantem que todos os microsserviços compartilhem a mesma estrutura de mensagens.

---

# Tecnologias Utilizadas

| Tecnologia           | Finalidade                     |
| -------------------- | ------------------------------ |
| .NET 10              | Plataforma principal           |
| ASP.NET Core Web API | Hospedagem da aplicação        |
| MediatR              | Implementação dos casos de uso |
| RabbitMQ             | Broker de mensagens            |
| MassTransit          | Consumo dos eventos            |
| Swagger              | Documentação da API            |
| Docker               | Containerização                |
| Kubernetes           | Orquestração dos containers    |
| ILogger              | Registro dos logs              |

---

# Fluxo de Processamento

Toda a NotificationsAPI é orientada a eventos.

Ela permanece aguardando mensagens publicadas pelos demais microsserviços e executa o processamento automaticamente quando um evento é recebido.

Existem atualmente dois fluxos independentes.

---

## Fluxo de Cadastro de Usuário

Quando um novo usuário é criado, a UsersAPI publica um evento no RabbitMQ.

```text
UsersAPI
      │
      │ UserCreatedEvent
      ▼
RabbitMQ
      │
      ▼
NotificationsAPI
      │
      ▼
UserCreatedEventConsumer
      │
      ▼
Registro do e-mail de boas-vindas
(simulado por logs)
```

Nesse fluxo não existe envio real de e-mail.

A aplicação apenas registra em log uma mensagem simulando o envio da notificação ao usuário recém-cadastrado.

---

## Fluxo de Confirmação de Compra

Após o processamento do pagamento, a PaymentsAPI publica um novo evento.

```text
PaymentsAPI
      │
      │ PaymentProcessedEvent
      ▼
RabbitMQ
      │
      ▼
NotificationsAPI
      │
      ▼
PaymentProcessedEventConsumer
      │
      ▼
SendPurchaseConfirmationCommand
      │
      ▼
SendPurchaseConfirmationCommandHandler
      │
      ├────────► Pagamento aprovado
      │              │
      │              ▼
      │      Simula confirmação da compra
      │
      └────────► Pagamento rejeitado
                     │
                     ▼
             Nenhuma notificação enviada
```

Quando o pagamento é aprovado, a aplicação registra um e-mail de confirmação de compra simulado.

Caso o pagamento seja rejeitado, nenhuma confirmação é enviada, sendo registrado apenas um log informando a rejeição.

---

## Resumo do Fluxo Geral

```text
UsersAPI
      │
      ▼
UserCreatedEvent
      │
      ▼
RabbitMQ
      │
      ▼
NotificationsAPI
      │
      ▼
E-mail de boas-vindas (simulado)

────────────────────────────────────

PaymentsAPI
      │
      ▼
PaymentProcessedEvent
      │
      ▼
RabbitMQ
      │
      ▼
NotificationsAPI
      │
      ▼
Confirmação de compra (simulada)
```

A NotificationsAPI atua exclusivamente como consumidora de eventos, mantendo a responsabilidade de notificação desacoplada dos demais microsserviços e permitindo que novas formas de comunicação sejam adicionadas futuramente sem alterar os serviços responsáveis pelo cadastro de usuários ou processamento de pagamentos.

# CQRS e MediatR

A NotificationsAPI utiliza **MediatR** para organizar os casos de uso relacionados ao processamento das notificações.

Embora o serviço não possua operações tradicionais de escrita e leitura em banco de dados, a separação dos casos de uso continua sendo aplicada.

O fluxo funciona da seguinte forma:

```text
Evento recebido
      │
      ▼
Consumer
      │
      ▼
Command
      │
      ▼
CommandHandler
      │
      ▼
Processamento da notificação
```

No cenário atual, o principal comando é:

```text
SendPurchaseConfirmationCommand
```

Ele representa a solicitação para processar a confirmação de uma compra após o recebimento de um `PaymentProcessedEvent`.

O comando contém:

* `OrderId`;
* `UserId`;
* `GameId`;
* `Price`;
* `Status`;
* `ProcessedAt`.

O processamento é executado pelo:

```text
SendPurchaseConfirmationCommandHandler
```

Esse handler verifica o status do pagamento e decide se a notificação deve ou não ser simulada.

---

## Por que não existe NotificationService?

A NotificationsAPI não possui um serviço genérico como:

```text
NotificationService
```

Isso ocorre porque os casos de uso são separados entre Consumers, Commands e Handlers.

As responsabilidades estão distribuídas da seguinte forma:

| Responsabilidade                 | Componente                               |
| -------------------------------- | ---------------------------------------- |
| Receber cadastro de usuário      | `UserCreatedEventConsumer`               |
| Simular boas-vindas              | `UserCreatedEventConsumer`               |
| Receber resultado de pagamento   | `PaymentProcessedEventConsumer`          |
| Encaminhar confirmação de compra | `SendPurchaseConfirmationCommand`        |
| Decidir se envia confirmação     | `SendPurchaseConfirmationCommandHandler` |
| Registrar notificações           | `ILogger`                                |

Dessa forma, cada classe possui uma responsabilidade específica.

Criar um `NotificationService` apenas para repetir essas operações adicionaria uma camada desnecessária e reduziria a separação proporcionada pelo MediatR.

---

# Consumers

Os Consumers são responsáveis por receber as mensagens entregues pelo RabbitMQ através do MassTransit.

A NotificationsAPI possui dois consumidores.

---

## UserCreatedEventConsumer

O `UserCreatedEventConsumer` recebe o evento publicado pela UsersAPI sempre que um novo usuário é cadastrado.

```text
UserCreatedEvent
      │
      ▼
UserCreatedEventConsumer
      │
      ▼
Simulação do e-mail de boas-vindas
```

O Consumer utiliza diretamente o `ILogger` para registrar a notificação simulada.

As informações registradas são:

* identificador do usuário;
* nome;
* e-mail;
* role;
* data de criação;
* mensagem de boas-vindas.

Exemplo de estrutura recebida:

```csharp
public sealed record UserCreatedEvent(
    Guid UserId,
    string Name,
    string Email,
    string Role,
    DateTime CreatedAt
);
```

O fluxo termina após o registro do log.

Não existe persistência, envio para serviço externo ou retorno HTTP.

---

## PaymentProcessedEventConsumer

O `PaymentProcessedEventConsumer` recebe o evento publicado pela PaymentsAPI após o processamento do pagamento.

```text
PaymentProcessedEvent
      │
      ▼
PaymentProcessedEventConsumer
      │
      ▼
SendPurchaseConfirmationCommand
      │
      ▼
SendPurchaseConfirmationCommandHandler
```

O Consumer executa as seguintes etapas:

1. recebe a mensagem do RabbitMQ;
2. registra os dados do evento em log;
3. cria um `SendPurchaseConfirmationCommand`;
4. envia o comando pelo MediatR;
5. recebe o resultado do processamento;
6. registra o resultado final em log.

O resultado contém:

```csharp
public record PurchaseConfirmationResult(
    PurchaseConfirmationStatus Status,
    bool NotificationSent);
```

Esse objeto informa:

* o resultado do processamento;
* se uma notificação foi simulada.

---

# Eventos

A NotificationsAPI atua exclusivamente como consumidora de eventos.

Ela não publica novos eventos no fluxo atual.

---

## UserCreatedEvent

Publicado pela UsersAPI após a criação de um usuário.

```csharp
public sealed record UserCreatedEvent(
    Guid UserId,
    string Name,
    string Email,
    string Role,
    DateTime CreatedAt
);
```

### Informações recebidas

| Campo       | Descrição                |
| ----------- | ------------------------ |
| `UserId`    | Identificador do usuário |
| `Name`      | Nome do usuário          |
| `Email`     | E-mail cadastrado        |
| `Role`      | Perfil do usuário        |
| `CreatedAt` | Data de criação          |

### Ação executada

```text
Simulação de e-mail de boas-vindas
```

---

## PaymentProcessedEvent

Publicado pela PaymentsAPI após o processamento do pagamento.

```csharp
public record PaymentProcessedEvent(
    Guid OrderId,
    Guid UserId,
    Guid GameId,
    decimal Price,
    string Status,
    DateTime ProcessedAt
);
```

### Informações recebidas

| Campo         | Descrição                |
| ------------- | ------------------------ |
| `OrderId`     | Identificador do pedido  |
| `UserId`      | Identificador do usuário |
| `GameId`      | Identificador do jogo    |
| `Price`       | Valor processado         |
| `Status`      | Status do pagamento      |
| `ProcessedAt` | Data do processamento    |

### Ação executada

O status define o comportamento da aplicação:

```text
Approved
    │
    ▼
Simulação de confirmação da compra
```

```text
Rejected
    │
    ▼
Nenhuma notificação enviada
```

---

# RabbitMQ e MassTransit

A NotificationsAPI utiliza o RabbitMQ como broker de mensagens e o MassTransit para configurar e executar os consumidores.

Os consumidores são registrados durante a inicialização da aplicação:

```csharp
config.AddConsumer<UserCreatedEventConsumer>();
config.AddConsumer<PaymentProcessedEventConsumer>();
```

O host é obtido através da seção:

```text
RabbitMq
```

As credenciais são fornecidas por configuração externa.

---

## Filas Utilizadas

A NotificationsAPI utiliza duas filas.

### notifications-user-created-event

Responsável pelo recebimento do:

```text
UserCreatedEvent
```

Configuração:

```csharp
cfg.ReceiveEndpoint(
    "notifications-user-created-event",
    endpoint =>
    {
        endpoint.ConfigureConsumer<
            UserCreatedEventConsumer>(context);
    });
```

---

### notifications-payment-processed-event

Responsável pelo recebimento do:

```text
PaymentProcessedEvent
```

Configuração:

```csharp
cfg.ReceiveEndpoint(
    "notifications-payment-processed-event",
    endpoint =>
    {
        endpoint.ConfigureConsumer<
            PaymentProcessedEventConsumer>(context);
    });
```

---

## Fluxo de Mensageria

```text
UsersAPI
      │
      │ publica UserCreatedEvent
      ▼
RabbitMQ
      │
      │ fila notifications-user-created-event
      ▼
NotificationsAPI
```

```text
PaymentsAPI
      │
      │ publica PaymentProcessedEvent
      ▼
RabbitMQ
      │
      │ fila notifications-payment-processed-event
      ▼
NotificationsAPI
```

Cada fila possui um Consumer específico, evitando que mensagens diferentes sejam tratadas pela mesma classe.

---

# Processamento das Notificações

A NotificationsAPI não envia e-mails reais.

Todo o processamento é simulado através de logs estruturados.

Isso permite validar:

* recebimento dos eventos;
* funcionamento do RabbitMQ;
* integração entre os microsserviços;
* execução dos Consumers;
* execução dos Handlers;
* decisões baseadas no status do pagamento.

---

## E-mail de Boas-Vindas

O fluxo é iniciado pelo `UserCreatedEvent`.

O Consumer registra uma mensagem semelhante a:

```text
=========================================
E-MAIL DE BOAS-VINDAS SIMULADO
UserId: ...
Name: ...
Email: ...
Role: ...
CreatedAt: ...
Mensagem: Bem-vindo à FIAP Cloud Games!
=========================================
```

Nenhum provedor SMTP ou serviço externo é utilizado.

---

## Confirmação de Compra

O fluxo é iniciado pelo `PaymentProcessedEvent`.

O `SendPurchaseConfirmationCommandHandler` compara o status recebido com:

```text
Approved
```

A comparação ignora diferenças entre letras maiúsculas e minúsculas.

---

## Pagamento Aprovado

Quando o status é aprovado:

```text
Status = Approved
```

A aplicação:

1. registra que o pagamento foi aprovado;
2. registra os dados da compra;
3. simula o e-mail de confirmação;
4. retorna `NotificationSent = true`.

Resultado:

```text
PurchaseConfirmationStatus.NotificationSent
NotificationSent: true
```

---

## Pagamento Rejeitado

Quando o status não é aprovado:

1. registra o pagamento como rejeitado;
2. informa que nenhuma notificação foi enviada;
3. retorna `NotificationSent = false`.

Resultado:

```text
PurchaseConfirmationStatus.PaymentRejected
NotificationSent: false
```

---

## Resultado do Processamento

O enum utilizado é:

```csharp
public enum PurchaseConfirmationStatus
{
    NotificationSent = 1,
    PaymentRejected = 2
}
```

O resultado final permite registrar claramente se a notificação foi ou não processada.

---

## Ausência de Persistência

A NotificationsAPI não armazena as notificações em banco de dados.

No fluxo atual:

* os eventos são recebidos;
* o processamento é realizado;
* o resultado é registrado em log;
* a mensagem é concluída.

Essa decisão mantém o microsserviço simples e adequado ao escopo atual do projeto.

Em uma evolução futura, o serviço poderia incorporar persistência, histórico, tentativas de reenvio ou integração com provedores externos, sem alterar os contratos dos microsserviços produtores.


# Endpoints

A NotificationsAPI não possui endpoints de negócio para criação ou envio manual de notificações.

O processamento ocorre exclusivamente pelo consumo de eventos publicados no RabbitMQ.

Os endpoints HTTP existentes são auxiliares e servem apenas para verificar se a aplicação está em execução.

---

## GET /

Retorna uma resposta simples informando que o microsserviço está ativo.

```http
GET /
```

### Exemplo com PowerShell

```powershell
curl http://localhost:8083/
```

### Resposta esperada

```json
{
  "service": "NotificationsAPI",
  "message": "FCG Notifications API is running."
}
```

Esse endpoint não verifica o funcionamento do RabbitMQ ou o consumo das filas. Ele apenas confirma que a aplicação está respondendo via HTTP.

---

## GET /health

Verifica a disponibilidade HTTP da NotificationsAPI.

```http
GET /health
```

### Exemplo com PowerShell

```powershell
curl http://localhost:8083/health
```

### Resposta esperada

```json
{
  "service": "NotificationsAPI",
  "status": "Healthy"
}
```

O health check atual não realiza uma verificação profunda das conexões com o RabbitMQ.

Para validar a mensageria, é necessário acompanhar os logs dos Consumers e verificar as filas no RabbitMQ Management.

---

# Configuração

A NotificationsAPI utiliza a seção `RabbitMq` para configurar a conexão com o broker.

Exemplo de configuração não sensível no `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "MassTransit": "Warning",
      "RabbitMQ.Client": "Warning"
    }
  },

  "AllowedHosts": "*",

  "RabbitMq": {
    "Host": "localhost"
  }
}
```

As credenciais do RabbitMQ não devem ser armazenadas diretamente no arquivo versionado.

---

## Variáveis de Ambiente

O ASP.NET Core utiliza dois caracteres de sublinhado para representar níveis hierárquicos de configuração.

| Variável                      | Finalidade                       |
| ----------------------------- | -------------------------------- |
| `RabbitMq__Host`              | Host do RabbitMQ                 |
| `RabbitMq__Username`          | Usuário do RabbitMQ              |
| `RabbitMq__Password`          | Senha do RabbitMQ                |
| `ASPNETCORE_ENVIRONMENT`      | Ambiente da aplicação            |
| `ASPNETCORE_HTTP_PORTS`       | Porta HTTP interna               |
| `DOTNET_RUNNING_IN_CONTAINER` | Identifica execução em container |

### Exemplo no PowerShell

```powershell
$env:RabbitMq__Host = "localhost"
$env:RabbitMq__Username = "<RABBITMQ_USER>"
$env:RabbitMq__Password = "<RABBITMQ_PASSWORD>"
```

Não publique credenciais reais no GitHub.

---

# Execução Local

## Pré-requisitos

Para executar a NotificationsAPI localmente, é necessário possuir:

* .NET SDK 10;
* RabbitMQ disponível;
* credenciais válidas;
* acesso às filas configuradas;
* contratos compatíveis com UsersAPI e PaymentsAPI.

A NotificationsAPI não utiliza SQL Server no fluxo atual.

---

## Acessar o Projeto

```powershell
cd D:\FIAP-FCG-MICROSERVICOS\FCG-Notifications-Api
```

---

## Restaurar as Dependências

```powershell
dotnet restore
```

---

## Compilar a Solução

```powershell
dotnet build
```

---

## Executar a Aplicação

```powershell
dotnet run --project src/FCG.Notifications.Api
```

Após a inicialização, o console deverá apresentar informações semelhantes a:

```text
Configured endpoint notifications-user-created-event
Configured endpoint notifications-payment-processed-event
Bus started
Application started
```

A porta utilizada localmente é definida pelo arquivo `launchSettings.json`.

No ambiente Docker, a aplicação utiliza a porta:

```text
8083
```

---

## Validar a Aplicação Localmente

Com a aplicação em execução:

```powershell
curl http://localhost:<PORTA>/health
```

Para validar o processamento das notificações, não basta acessar o endpoint HTTP.

É necessário:

1. criar um usuário na UsersAPI; ou
2. concluir um pagamento pela PaymentsAPI;
3. acompanhar os logs da NotificationsAPI.

---

# Docker

A NotificationsAPI utiliza um Dockerfile com múltiplos estágios.

O processo é dividido em:

```text
Restore
   |
   v
Build e Publish
   |
   v
Imagem final ASP.NET Runtime
```

A imagem final utiliza:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
```

A porta interna configurada é:

```dockerfile
ENV ASPNETCORE_HTTP_PORTS=8083
EXPOSE 8083
```

O ambiente padrão do container é:

```dockerfile
ENV ASPNETCORE_ENVIRONMENT=Production
```

---

## Criar a Imagem

Na raiz do repositório:

```powershell
docker build -t fcg-notifications-api:1.0 .
```

---

## Executar Isoladamente

A execução isolada exige acesso ao RabbitMQ.

```powershell
docker run --rm `
  --name fcg-notifications-api `
  -p 8083:8083 `
  -e RabbitMq__Host="<RABBITMQ_HOST>" `
  -e RabbitMq__Username="<RABBITMQ_USER>" `
  -e RabbitMq__Password="<RABBITMQ_PASSWORD>" `
  fcg-notifications-api:1.0
```

Caso o RabbitMQ esteja em outro container, ambos devem estar na mesma rede Docker.

No ambiente completo, recomenda-se utilizar o repositório de orquestração.

---

## Executar com Docker Compose

```powershell
cd D:\FIAP-FCG-MICROSERVICOS\FCG-Orchestration-Api
docker compose config
docker compose build
docker compose up -d
docker compose ps
```

A NotificationsAPI ficará disponível em:

```text
http://localhost:8083
```

---

## Validar o Container

```powershell
curl http://localhost:8083/
curl http://localhost:8083/health
```

---

## Acompanhar os Logs

```powershell
docker compose logs -f notifications-api
```

Para acompanhar o fluxo completo da compra:

```powershell
docker compose logs -f --tail=0 catalog-api payments-api notifications-api
```

Para acompanhar o fluxo de cadastro:

```powershell
docker compose logs -f --tail=0 users-api notifications-api
```

---

## Parar o Ambiente

```powershell
docker compose down
```

Esse comando remove os containers e redes criados pelo Compose, mas mantém os volumes.

Para remover também os volumes:

```powershell
docker compose down -v
```

O parâmetro `-v` remove os dados persistidos do SQL Server e RabbitMQ utilizados pelos demais serviços.

Use esse comando apenas quando desejar reinicializar completamente o ambiente.

---

# Kubernetes

Os arquivos Kubernetes da NotificationsAPI são centralizados no repositório:

```text
FCG-Orchestration-Api
```

Docker Compose e Kubernetes são formas alternativas de execução.

Não é necessário executar os dois ambientes ao mesmo tempo.

---

## Aplicar os Manifestos

```powershell
cd D:\FIAP-FCG-MICROSERVICOS\FCG-Orchestration-Api
kubectl apply -f k8s/
```

---

## Verificar os Pods

```powershell
kubectl get pods
```

Para acompanhar os pods em tempo real:

```powershell
kubectl get pods -w
```

---

## Verificar o Deployment

```powershell
kubectl get deployment notifications-api
```

---

## Verificar os Services

```powershell
kubectl get services
```

---

## Consultar os Logs

```powershell
kubectl logs deployment/notifications-api
```

Para acompanhar continuamente:

```powershell
kubectl logs -f deployment/notifications-api
```

---

## Reiniciar a NotificationsAPI

```powershell
kubectl rollout restart deployment notifications-api
```

---

## Acompanhar o Reinício

```powershell
kubectl rollout status deployment/notifications-api
```

---

## Remover o Ambiente

```powershell
kubectl delete -f k8s/
```

---

## ConfigMap

As configurações não sensíveis podem ser armazenadas no ConfigMap.

Exemplo:

```text
ASPNETCORE_ENVIRONMENT
RabbitMq__Host
```

---

## Secret

Informações sensíveis devem ser armazenadas no Kubernetes Secret.

Exemplo:

```text
RabbitMq__Username
RabbitMq__Password
```

Não armazene credenciais reais diretamente nos manifestos versionados ou no README.

---

# Swagger

A NotificationsAPI possui Swagger configurado através de:

```csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
```

O Swagger é disponibilizado somente quando a aplicação está em ambiente de desenvolvimento:

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

Quando a aplicação estiver em `Development`, utilize:

```text
http://localhost:<PORTA>/swagger
```

Caso a aplicação esteja em desenvolvimento na porta 8083:

```text
http://localhost:8083/swagger
```

---

## Swagger no Docker

O Dockerfile define:

```dockerfile
ENV ASPNETCORE_ENVIRONMENT=Production
```

Por esse motivo, o Swagger não estará disponível no container enquanto o ambiente permanecer como `Production`.

Esse comportamento não impede o funcionamento da aplicação, pois a NotificationsAPI não possui endpoints de negócio para o cliente.

Para validar o container, utilize:

```powershell
curl http://localhost:8083/
curl http://localhost:8083/health
```

A validação principal das notificações deve ser feita pelos logs e pelas filas do RabbitMQ.

---

# Autenticação

A NotificationsAPI não configura autenticação JWT no fluxo atual.

Isso ocorre porque:

* o cadastro é realizado na UsersAPI;
* a compra é iniciada na CatalogAPI;
* o pagamento é processado na PaymentsAPI;
* a NotificationsAPI recebe apenas eventos internos pelo RabbitMQ.

As credenciais utilizadas por este serviço são as credenciais de conexão com o broker, e não tokens de usuários.

Portanto:

* não existe login na NotificationsAPI;
* não é necessário enviar JWT;
* não existem endpoints protegidos;
* os Consumers recebem mensagens diretamente do RabbitMQ.


# Logs Esperados

A NotificationsAPI utiliza o **ILogger** para registrar todas as etapas do processamento das notificações.

Como o envio de e-mails é apenas simulado, os logs representam a principal forma de validar o funcionamento da aplicação e a integração com os demais microsserviços.

---

## Recebimento do UserCreatedEvent

Sempre que um novo usuário é cadastrado na UsersAPI, o evento `UserCreatedEvent` é consumido automaticamente.

O log esperado é semelhante ao seguinte:

```text
=========================================
E-MAIL DE BOAS-VINDAS SIMULADO
UserId: ...
Name: ...
Email: ...
Role: ...
CreatedAt: ...
Mensagem: Bem-vindo à FIAP Cloud Games!
=========================================
```

Esse registro confirma que:

* o evento foi publicado pela UsersAPI;
* o RabbitMQ entregou a mensagem corretamente;
* o Consumer executou a simulação do envio do e-mail.

---

## Recebimento do PaymentProcessedEvent

Após a conclusão do pagamento, a PaymentsAPI publica um `PaymentProcessedEvent`.

Ao consumir esse evento, a NotificationsAPI registra:

```text
PaymentProcessedEvent recebido.
OrderId: ...
UserId: ...
GameId: ...
Price: ...
Status: Approved
ProcessedAt: ...
```

Esse log confirma que o evento chegou corretamente ao microsserviço.

---

## Pagamento Aprovado

Quando o pagamento possui status **Approved**, o Handler executa a simulação da confirmação da compra.

Logs esperados:

```text
Pagamento aprovado.
OrderId: ...
UserId: ...
GameId: ...
Price: ...
Status: Approved
```

Em seguida:

```text
=========================================
E-MAIL DE CONFIRMAÇÃO DE COMPRA SIMULADO
OrderId: ...
UserId: ...
GameId: ...
Price: ...
Status: Approved
ProcessedAt: ...
=========================================
```

Finalizando com:

```text
E-mail de confirmação de compra simulado para o usuário ...
```

---

## Pagamento Rejeitado

Caso o pagamento seja rejeitado:

```text
Pagamento rejeitado.
OrderId: ...
UserId: ...
GameId: ...
Price: ...
Status: Rejected
```

Em seguida:

```text
Nenhuma notificação enviada.
OrderId: ...
Status: Rejected
```

Nesse cenário, nenhuma confirmação de compra é simulada.

---

## Processamento Finalizado

Após o Handler concluir o processamento, o Consumer registra:

```text
PaymentProcessedEvent processado com sucesso.
OrderId: ...
ProcessingResult: NotificationSent
NotificationSent: True
```

ou

```text
PaymentProcessedEvent processado com sucesso.
ProcessingResult: PaymentRejected
NotificationSent: False
```

Esse log representa o encerramento do processamento da mensagem.

---

## Consultando os Logs

### Docker Compose

```powershell
docker compose logs -f notifications-api
```

Para acompanhar todo o fluxo da aplicação:

```powershell
docker compose logs -f --tail=0 users-api catalog-api payments-api notifications-api
```

---

### Kubernetes

```powershell
kubectl logs deployment/notifications-api
```

Para acompanhar continuamente:

```powershell
kubectl logs -f deployment/notifications-api
```

---

# Troubleshooting

## RabbitMQ indisponível

Sintomas:

* Consumers não iniciam;
* mensagens permanecem na fila;
* exceção **Broker Unreachable**.

Verifique:

```powershell
docker compose ps
docker compose logs rabbitmq
```

Confirme:

* RabbitMq__Host;
* RabbitMq__Username;
* RabbitMq__Password;
* porta **5672**.

---

## Consumer não recebe mensagens

Caso nenhuma notificação seja processada:

* confirme que a UsersAPI publicou `UserCreatedEvent`;
* confirme que a PaymentsAPI publicou `PaymentProcessedEvent`;
* verifique se as filas foram criadas;
* confirme se os Consumers estão conectados.

As filas esperadas são:

```text
notifications-user-created-event
notifications-payment-processed-event
```

---

## Contrato incompatível

Caso os eventos não sejam desserializados corretamente:

* confirme que os contratos compartilhados são idênticos entre os microsserviços;
* verifique namespaces e propriedades dos eventos.

Qualquer incompatibilidade impedirá o processamento da mensagem.

---

## Swagger indisponível

O Dockerfile define:

```dockerfile
ASPNETCORE_ENVIRONMENT=Production
```

Como consequência, o Swagger não será disponibilizado no container.

Para validar a aplicação utilize:

```powershell
curl http://localhost:8083/
curl http://localhost:8083/health
```

---

## Porta 8083 em uso

Localize o processo utilizando:

```powershell
netstat -ano | findstr :8083
```

Finalize o processo:

```powershell
taskkill /PID <PID> /F
```

Ou altere o mapeamento de portas do Docker Compose.

---

## Nenhuma notificação aparece

Se os endpoints responderem normalmente, mas não existirem logs de notificação:

1. confirme se o RabbitMQ está ativo;
2. verifique se o evento foi publicado;
3. confirme se o Consumer iniciou corretamente;
4. consulte os logs da NotificationsAPI;
5. valide se o pagamento foi realmente aprovado.

---

# Checklist de Validação

Antes de considerar a NotificationsAPI pronta para utilização, confirme os seguintes itens.

## Infraestrutura

* [ ] RabbitMQ em execução.
* [ ] Filas criadas corretamente.
* [ ] Consumers conectados.

---

## Aplicação

* [ ] Projeto compila sem erros.
* [ ] NotificationsAPI inicia corretamente.
* [ ] Endpoint `/` responde.
* [ ] Health Check retorna **Healthy**.
* [ ] Swagger disponível em ambiente Development.

---

## Integração

* [ ] UsersAPI publica `UserCreatedEvent`.
* [ ] PaymentsAPI publica `PaymentProcessedEvent`.
* [ ] NotificationsAPI recebe ambos os eventos.

---

## Notificações

* [ ] Boas-vindas simuladas após cadastro.
* [ ] Confirmação simulada após pagamento aprovado.
* [ ] Nenhuma confirmação enviada para pagamentos rejeitados.

---

## Logs

* [ ] Recebimento dos eventos registrado.
* [ ] Processamento registrado.
* [ ] Resultado final registrado.

---

# Conclusão

A **FCG Notifications API** é o microsserviço responsável pelo processamento das notificações da plataforma **FIAP Cloud Games**, atuando de forma totalmente desacoplada dos demais serviços por meio de mensageria.

Sua arquitetura baseada em **DDD**, **CQRS**, **MediatR**, **RabbitMQ** e **MassTransit** permite que novos tipos de notificações sejam incorporados sem impactar os microsserviços responsáveis pelo cadastro de usuários ou pelo processamento dos pagamentos.

No escopo atual do projeto, a NotificationsAPI:

* consome o `UserCreatedEvent` para simular o envio de e-mails de boas-vindas;
* consome o `PaymentProcessedEvent` para simular confirmações de compra;
* registra todo o processamento através de logs estruturados;
* não realiza persistência de dados;
* não envia e-mails reais;
* não exige autenticação JWT;
* não expõe endpoints de negócio.

Essa abordagem mantém o serviço simples, desacoplado e preparado para futuras evoluções, como integração com provedores SMTP, serviços de e-mail transacional, notificações push ou sistemas de mensageria, preservando os contratos já estabelecidos entre os microsserviços da solução.
