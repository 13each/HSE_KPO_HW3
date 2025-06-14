ARG APP_UID=1000
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["OrdersService.Api/OrdersService.Api.csproj", "OrdersService.Api/"]
COPY ["Common.Messages/Common.Messages.csproj", "Common.Messages/"]
COPY ["OrdersService.Infrastructure/OrdersService.Infrastructure.csproj", "OrdersService.Infrastructure/"]
COPY ["OrdersService.Domain/OrdersService.Domain.csproj", "OrdersService.Domain/"]
RUN dotnet restore "OrdersService.Api/OrdersService.Api.csproj"
COPY . .
WORKDIR "/src/OrdersService.Api"
RUN dotnet build "OrdersService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "OrdersService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrdersService.Api.dll"]
