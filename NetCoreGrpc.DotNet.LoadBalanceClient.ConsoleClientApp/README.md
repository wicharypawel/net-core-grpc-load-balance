# Thick client for gRPC dotnet

__NOTE: Run commands in root directory__

## Build images
```
docker build -t grpc-dotnet-client-round-robin:latest -f .\NetCoreGrpc.DotNet.LoadBalanceClient.ConsoleClientApp\Dockerfile .
docker build -t grpc-server:latest -f .\NetCoreGrpc.ServerApp\Dockerfile .
```

## Create resources in K8s
```
kubectl apply -f .\k8s\grpc-server.yaml
kubectl create -f .\k8s\grpc-dotnet-client-round-robin.yaml
```

## Verify connection
```
kubectl logs grpc-dotnet-client-round-robin
```

## Tear down resources
```
kubectl delete -f .\k8s\grpc-dotnet-client-round-robin.yaml
kubectl delete -f .\k8s\grpc-server.yaml
```

## Verify DNS SRV records
```
kubectl apply -f .\utils\dnsutils.yaml
kubectl exec -ti dnsutils -- nslookup -type=SRV _grpc._tcp.grpc-server.default.svc.cluster.local
kubectl delete -f .\utils\dnsutils.yaml
```