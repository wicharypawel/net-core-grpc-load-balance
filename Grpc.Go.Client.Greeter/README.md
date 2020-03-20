# Simple greeter for gRPC Go client

__NOTE: Run commands in root directory__

## Build images
Build image `go-grpc-client-greeter` using [this](https://github.com/wicharypawel/go-grpc-loadbalancing) repository
```
docker build -t grpc-server:latest -f .\NetCoreGrpc.ServerApp\Dockerfile .
```

## Create resources in K8s
```
kubectl apply -f .\k8s\grpc-server.yaml
kubectl create -f .\k8s\go-grpc-client-greeter.yaml
```

## Verify connection
```
kubectl logs go-grpc-client-greeter
```

## Tear down resources
```
kubectl delete -f .\k8s\go-grpc-client-greeter.yaml
kubectl delete -f .\k8s\grpc-server.yaml
```