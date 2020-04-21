#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["NetCoreGrpc.DotNet.LoadBalanceClient.ConsoleClientApp/NetCoreGrpc.DotNet.LoadBalanceClient.ConsoleClientApp.csproj", "NetCoreGrpc.DotNet.LoadBalanceClient.ConsoleClientApp/"]
COPY ["grpc-dotnet/src/Grpc.Net.Client/Grpc.Net.Client.csproj", "grpc-dotnet/src/Grpc.Net.Client/"]
COPY ["grpc-dotnet/src/Grpc.Net.Common/Grpc.Net.Common.csproj", "grpc-dotnet/src/Grpc.Net.Common/"]
RUN dotnet restore "NetCoreGrpc.DotNet.LoadBalanceClient.ConsoleClientApp/NetCoreGrpc.DotNet.LoadBalanceClient.ConsoleClientApp.csproj"
COPY . .
WORKDIR "/src/NetCoreGrpc.DotNet.LoadBalanceClient.ConsoleClientApp"
RUN dotnet build "NetCoreGrpc.DotNet.LoadBalanceClient.ConsoleClientApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NetCoreGrpc.DotNet.LoadBalanceClient.ConsoleClientApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NetCoreGrpc.DotNet.LoadBalanceClient.ConsoleClientApp.dll"]