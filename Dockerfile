FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /src

COPY ["src/FCG.Notifications.Api/FCG.Notifications.Api.csproj", "src/FCG.Notifications.Api/"]
COPY ["src/FCG.Notifications.Application/FCG.Notifications.Application.csproj", "src/FCG.Notifications.Application/"]
COPY ["src/FCG.Notifications.Contracts/FCG.Notifications.Contracts.csproj", "src/FCG.Notifications.Contracts/"]

RUN dotnet restore \
    "src/FCG.Notifications.Api/FCG.Notifications.Api.csproj"

FROM restore AS build

COPY . .

RUN dotnet publish \
    "src/FCG.Notifications.Api/FCG.Notifications.Api.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8083
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

EXPOSE 8083

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FCG.Notifications.Api.dll"]