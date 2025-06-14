ARG APP_UID=1000
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["PaymentsService.Api/PaymentsService.Api.csproj", "PaymentsService.Api/"]
COPY ["PaymentsService.Infrastructure/PaymentsService.Infrastructure.csproj", "PaymentsService.Infrastructure/"]
COPY ["Common.Messages/Common.Messages.csproj", "Common.Messages/"]
COPY ["PaymentsService.Domain/PaymentsService.Domain.csproj", "PaymentsService.Domain/"]
RUN dotnet restore "PaymentsService.Api/PaymentsService.Api.csproj"
COPY . .
WORKDIR "/src/PaymentsService.Api"
RUN dotnet build "PaymentsService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "PaymentsService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PaymentsService.Api.dll"]
