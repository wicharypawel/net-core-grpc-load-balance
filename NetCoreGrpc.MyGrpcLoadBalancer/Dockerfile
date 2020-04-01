#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 9000 9100
ENV ASPNETCORE_ENVIRONMENT Production

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["NetCoreGrpc.MyGrpcLoadBalancer/NetCoreGrpc.MyGrpcLoadBalancer.csproj", "NetCoreGrpc.MyGrpcLoadBalancer/"]
RUN dotnet restore "NetCoreGrpc.MyGrpcLoadBalancer/NetCoreGrpc.MyGrpcLoadBalancer.csproj"
COPY . .
WORKDIR "/src/NetCoreGrpc.MyGrpcLoadBalancer"
RUN dotnet build "NetCoreGrpc.MyGrpcLoadBalancer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NetCoreGrpc.MyGrpcLoadBalancer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NetCoreGrpc.MyGrpcLoadBalancer.dll"]