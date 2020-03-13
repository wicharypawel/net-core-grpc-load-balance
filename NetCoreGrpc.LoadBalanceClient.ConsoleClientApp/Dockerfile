#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["NetCoreGrpc.LoadBalanceClient.ConsoleClientApp/NetCoreGrpc.LoadBalanceClient.ConsoleClientApp.csproj", "NetCoreGrpc.LoadBalanceClient.ConsoleClientApp/"]
RUN dotnet restore "NetCoreGrpc.LoadBalanceClient.ConsoleClientApp/NetCoreGrpc.LoadBalanceClient.ConsoleClientApp.csproj"
COPY . .
WORKDIR "/src/NetCoreGrpc.LoadBalanceClient.ConsoleClientApp"
RUN dotnet build "NetCoreGrpc.LoadBalanceClient.ConsoleClientApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NetCoreGrpc.LoadBalanceClient.ConsoleClientApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NetCoreGrpc.LoadBalanceClient.ConsoleClientApp.dll"]