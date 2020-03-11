#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 9000
ENV ASPNETCORE_URLS http://*:9000

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["NetCoreGrpc.LoadBalanceExternal.GrpcLoadBalancer/NetCoreGrpc.LoadBalanceExternal.GrpcLoadBalancer.csproj", "NetCoreGrpc.LoadBalanceExternal.GrpcLoadBalancer/"]
RUN dotnet restore "NetCoreGrpc.LoadBalanceExternal.GrpcLoadBalancer/NetCoreGrpc.LoadBalanceExternal.GrpcLoadBalancer.csproj"
COPY . .
WORKDIR "/src/NetCoreGrpc.LoadBalanceExternal.GrpcLoadBalancer"
RUN dotnet build "NetCoreGrpc.LoadBalanceExternal.GrpcLoadBalancer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NetCoreGrpc.LoadBalanceExternal.GrpcLoadBalancer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NetCoreGrpc.LoadBalanceExternal.GrpcLoadBalancer.dll"]