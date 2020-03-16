Execute in root directory

```
docker build -t grpc-dotnet-client-round-robin:latest -f .\NetCoreGrpc.DotNet.LoadBalanceClient.ConsoleClientApp\Dockerfile .
docker build -t grpc-server:latest -f .\NetCoreGrpc.ServerApp\Dockerfile .
```

```
kubectl apply -f .\k8s\grpc-server.yaml
kubectl create -f .\k8s\grpc-dotnet-client-round-robin.yaml
```

```
kubectl logs grpc-dotnet-client-round-robin
```

```
kubectl delete -f .\k8s\grpc-dotnet-client-round-robin.yaml
kubectl delete -f .\k8s\grpc-server.yaml
```
