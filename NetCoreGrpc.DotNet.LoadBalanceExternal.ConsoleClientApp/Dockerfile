#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["NetCoreGrpc.DotNet.LoadBalanceExternal.ConsoleClientApp/NetCoreGrpc.DotNet.LoadBalanceExternal.ConsoleClientApp.csproj", "NetCoreGrpc.DotNet.LoadBalanceExternal.ConsoleClientApp/"]
COPY ["grpc-dotnet/src/Grpc.Net.Client.LoadBalancing.Extensions/Grpc.Net.Client.LoadBalancing.Extensions.csproj", "grpc-dotnet/src/Grpc.Net.Client.LoadBalancing.Extensions/"]
COPY ["grpc-dotnet/src/Grpc.Net.Client.LoadBalancing.Protos/Grpc.Net.Client.LoadBalancing.Protos.csproj", "grpc-dotnet/src/Grpc.Net.Client.LoadBalancing.Protos/"]
COPY ["grpc-dotnet/src/Grpc.Net.Client/Grpc.Net.Client.csproj", "grpc-dotnet/src/Grpc.Net.Client/"]
COPY ["grpc-dotnet/src/Grpc.Net.Common/Grpc.Net.Common.csproj", "grpc-dotnet/src/Grpc.Net.Common/"]
RUN dotnet restore "NetCoreGrpc.DotNet.LoadBalanceExternal.ConsoleClientApp/NetCoreGrpc.DotNet.LoadBalanceExternal.ConsoleClientApp.csproj"
COPY . .
WORKDIR "/src/NetCoreGrpc.DotNet.LoadBalanceExternal.ConsoleClientApp"
RUN dotnet build "NetCoreGrpc.DotNet.LoadBalanceExternal.ConsoleClientApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NetCoreGrpc.DotNet.LoadBalanceExternal.ConsoleClientApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NetCoreGrpc.DotNet.LoadBalanceExternal.ConsoleClientApp.dll"]