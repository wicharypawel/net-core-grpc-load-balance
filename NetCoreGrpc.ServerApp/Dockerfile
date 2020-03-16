#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 8000
EXPOSE 8001
ENV ASPNETCORE_URLS https://*:8001;http://*:8000
ENV ASPNETCORE_ENVIRONMENT Development

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["NetCoreGrpc.ServerApp/NetCoreGrpc.ServerApp.csproj", "NetCoreGrpc.ServerApp/"]
RUN dotnet restore "NetCoreGrpc.ServerApp/NetCoreGrpc.ServerApp.csproj"
COPY . .
WORKDIR "/src/NetCoreGrpc.ServerApp"
RUN dotnet build "NetCoreGrpc.ServerApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NetCoreGrpc.ServerApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NetCoreGrpc.ServerApp.dll"]