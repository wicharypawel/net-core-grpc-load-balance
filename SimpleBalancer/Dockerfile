FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 9000 9100
ENV ASPNETCORE_ENVIRONMENT Production

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["SimpleBalancer/SimpleBalancer.csproj", "SimpleBalancer/"]
RUN dotnet restore "SimpleBalancer/SimpleBalancer.csproj"
COPY . .
WORKDIR "/src/SimpleBalancer"
RUN dotnet build "SimpleBalancer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SimpleBalancer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SimpleBalancer.dll"]