Execute in root directory

```
docker build -t grpc-csharp-client-round-robin:latest -f .\NetCoreGrpc.LoadBalanceClient.ConsoleClientApp\Dockerfile .
docker build -t grpc-server:latest -f .\NetCoreGrpc.ServerApp\Dockerfile .
```

```
kubectl apply -f .\k8s\grpc-server.yaml
kubectl create -f .\k8s\grpc-csharp-client-round-robin.yaml
```

```
kubectl logs grpc-csharp-client-round-robin
```

```
kubectl delete -f .\k8s\grpc-csharp-client-round-robin.yaml
kubectl delete -f .\k8s\grpc-server.yaml
```
