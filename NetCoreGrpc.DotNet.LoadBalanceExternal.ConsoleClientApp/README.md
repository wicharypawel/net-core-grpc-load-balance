Execute in root directory

```
docker build -t grpc-dotnet-client-lookaside:latest -f .\NetCoreGrpc.DotNet.LoadBalanceExternal.ConsoleClientApp\Dockerfile .
docker build -t grpc-server-balancer:latest -f .\NetCoreGrpc.MyGrpcLoadBalancer\Dockerfile .
docker build -t grpc-server:latest -f .\NetCoreGrpc.ServerApp\Dockerfile .
```

```
kubectl apply -f .\k8s\grpc-server.yaml
kubectl apply -f .\k8s\grpc-server-balancer.yaml
kubectl create -f .\k8s\grpc-dotnet-client-lookaside.yaml
```

```
kubectl logs grpc-dotnet-client-lookaside
```

```
kubectl delete -f .\k8s\grpc-dotnet-client-lookaside.yaml
kubectl delete -f .\k8s\grpc-server-balancer.yaml
kubectl delete -f .\k8s\grpc-server.yaml
```
