FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["src/Keda.Durable.Scaler.Server/Keda.Durable.Scaler.Server.csproj", "src/Keda.Durable.Scaler.Server/"]
RUN dotnet restore "src/Keda.Durable.Scaler.Server/Keda.Durable.Scaler.Server.csproj"
COPY . .
WORKDIR "/src/src/Keda.Durable.Scaler.Server"
RUN dotnet build "Keda.Durable.Scaler.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Keda.Durable.Scaler.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Keda.Durable.Scaler.Server.dll"]