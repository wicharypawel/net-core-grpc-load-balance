# Round robin DNS load balancing for gRPC Go client

__NOTE: Run commands in root directory__

## Build images
```
Build image `go-grpc-client-round-robin` using [this](https://github.com/wicharypawel/go-grpc-loadbalancing) repository
docker build -t grpc-server:latest -f .\NetCoreGrpc.ServerApp\Dockerfile .
```

## Create resources in K8s
```
kubectl apply -f .\k8s\grpc-server.yaml
kubectl create -f .\k8s\go-grpc-client-round-robin.yaml
```

## Verify connection
```
kubectl logs go-grpc-client-round-robin
```

## Tear down resources
```
kubectl delete -f .\k8s\go-grpc-client-round-robin.yaml
kubectl delete -f .\k8s\grpc-server.yaml
```

## Verify DNS SRV records
```
kubectl apply -f .\utils\dnsutils.yaml
kubectl exec -ti dnsutils -- nslookup -type=SRV _grpc._tcp.grpc-server.default.svc.cluster.local
kubectl delete -f .\utils\dnsutils.yaml
```