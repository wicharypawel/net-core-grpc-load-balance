#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 8000
ENV ASPNETCORE_URLS http://*:8000

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["NetCoreGrpc.LoadBalanceExternal.AspNetCoreServerApp/NetCoreGrpc.LoadBalanceExternal.AspNetCoreServerApp.csproj", "NetCoreGrpc.LoadBalanceExternal.AspNetCoreServerApp/"]
RUN dotnet restore "NetCoreGrpc.LoadBalanceExternal.AspNetCoreServerApp/NetCoreGrpc.LoadBalanceExternal.AspNetCoreServerApp.csproj"
COPY . .
WORKDIR "/src/NetCoreGrpc.LoadBalanceExternal.AspNetCoreServerApp"
RUN dotnet build "NetCoreGrpc.LoadBalanceExternal.AspNetCoreServerApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NetCoreGrpc.LoadBalanceExternal.AspNetCoreServerApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NetCoreGrpc.LoadBalanceExternal.AspNetCoreServerApp.dll"]