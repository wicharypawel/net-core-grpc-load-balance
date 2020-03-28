#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS http://*:5000
ENV ASPNETCORE_ENVIRONMENT Development

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["NetCoreGrpc.DotNet.LoadBalanceExternal.AspNetClientApp/NetCoreGrpc.DotNet.LoadBalanceExternal.AspNetClientApp.csproj", "NetCoreGrpc.DotNet.LoadBalanceExternal.AspNetClientApp/"]
COPY ["grpc-dotnet/src/Grpc.Net.ClientFactory/Grpc.Net.ClientFactory.csproj", "grpc-dotnet/src/Grpc.Net.ClientFactory/"]
COPY ["grpc-dotnet/src/Grpc.Net.Client/Grpc.Net.Client.csproj", "grpc-dotnet/src/Grpc.Net.Client/"]
COPY ["grpc-dotnet/src/Grpc.Net.Common/Grpc.Net.Common.csproj", "grpc-dotnet/src/Grpc.Net.Common/"]
COPY ["grpc-dotnet/src/Grpc.AspNetCore/Grpc.AspNetCore.csproj", "grpc-dotnet/src/Grpc.AspNetCore/"]
COPY ["grpc-dotnet/src/Grpc.AspNetCore.Server.ClientFactory/Grpc.AspNetCore.Server.ClientFactory.csproj", "grpc-dotnet/src/Grpc.AspNetCore.Server.ClientFactory/"]
COPY ["grpc-dotnet/src/Grpc.AspNetCore.Server/Grpc.AspNetCore.Server.csproj", "grpc-dotnet/src/Grpc.AspNetCore.Server/"]
RUN dotnet restore "NetCoreGrpc.DotNet.LoadBalanceExternal.AspNetClientApp/NetCoreGrpc.DotNet.LoadBalanceExternal.AspNetClientApp.csproj"
COPY . .
WORKDIR "/src/NetCoreGrpc.DotNet.LoadBalanceExternal.AspNetClientApp"
RUN dotnet build "NetCoreGrpc.DotNet.LoadBalanceExternal.AspNetClientApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NetCoreGrpc.DotNet.LoadBalanceExternal.AspNetClientApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NetCoreGrpc.DotNet.LoadBalanceExternal.AspNetClientApp.dll"]