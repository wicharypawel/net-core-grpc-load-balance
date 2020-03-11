FROM microsoft/dotnet:2.0-sdk
WORKDIR /app

ADD . /app

RUN dotnet publish -o out -c Release -f netcoreapp2.0 NetCoreGrpc.LoadBalanceExternal.AspNetCoreServerApp.csproj

ENTRYPOINT ["dotnet", "exec", "/app/out/NetCoreGrpc.LoadBalanceExternal.AspNetCoreServerApp.dll"]
