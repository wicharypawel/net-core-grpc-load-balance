# Lookaside load balancing for gRPC dotnet client with custom dns server (CoreDNS)

## Overview

![Overview](./overview.PNG)

__NOTE: Run commands in root directory__

__NOTE: K8s files works with local docker images, change imagePullPolicy to allow remote registry__

## Build images
```
docker build -t grpc-dotnet-client-lookaside:latest -f .\NetCoreGrpc.DotNet.LoadBalanceExternal.ConsoleClientApp\Dockerfile .
docker build -t grpc-server-balancer:latest -f .\NetCoreGrpc.MyGrpcLoadBalancer\Dockerfile .
docker build -t grpc-server:latest -f .\NetCoreGrpc.ServerApp\Dockerfile .
```

## Create resources in K8s
```
kubectl apply -f .\k8s\custom-dns.yaml
kubectl apply -f .\k8s\grpc-server.yaml
kubectl apply -f .\k8s\grpc-server-balancer.yaml
kubectl create -f .\k8s\grpc-dotnet-client-lookaside-custom-dns.yaml
```

## Verify connection
```
kubectl logs grpc-dotnet-client-lookaside-custom-dns
```

## Tear down resources
```
kubectl delete -f .\k8s\grpc-dotnet-client-lookaside-custom-dns.yaml
kubectl delete -f .\k8s\grpc-server-balancer.yaml
kubectl delete -f .\k8s\grpc-server.yaml
kubectl delete -f .\k8s\custom-dns.yaml
```

## Verify DNS SRV records
```
kubectl apply -f .\utils\dnsutils.yaml
kubectl exec -ti dnsutils -- nslookup -type=SRV _grpclb._tcp.grpc-server-balancer.default.svc.cluster.local
kubectl exec -ti dnsutils -- dig @custom-dns -p 53 txt _grpc_config.grpc-server.custom-dns.local
kubectl delete -f .\utils\dnsutils.yaml
```
