# Lookaside load balancing for gRPC Python client

## Overview

![Overview](./overview.PNG)

__NOTE: Run commands in root directory__

__NOTE: K8s files works with local docker images, change imagePullPolicy to allow remote registry__

# WORK IN PROCESS EXAMPLE NOT COMPLETED

## Build images
Build image `python-grpc-client-lookaside` using ...
```
docker build -t grpc-server-balancer:latest -f .\NetCoreGrpc.MyGrpcLoadBalancer\Dockerfile .
docker build -t grpc-server:latest -f .\NetCoreGrpc.ServerApp\Dockerfile .
```

## Create resources in K8s
```
kubectl apply -f .\k8s\grpc-server.yaml
kubectl apply -f .\k8s\grpc-server-balancer.yaml
kubectl create -f .\k8s\python-grpc-client-lookaside.yaml
```

## Verify connection
```
kubectl logs python-grpc-client-lookaside
```

## Tear down resources
```
kubectl delete -f .\k8s\python-grpc-client-lookaside.yaml
kubectl delete -f .\k8s\grpc-server-balancer.yaml
kubectl delete -f .\k8s\grpc-server.yaml
```

## Verify DNS SRV records
```
kubectl apply -f .\utils\dnsutils.yaml
kubectl exec -ti dnsutils -- nslookup -type=SRV _grpclb._tcp.grpc-server-balancer.default.svc.cluster.local
kubectl delete -f .\utils\dnsutils.yaml
```
