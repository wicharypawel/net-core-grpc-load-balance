Execute in root directory

```
docker build -t wicharypawel/grpc-loadbalancing_greeter_client_round_robin:latest -f .\NetCoreGrpc.LoadBalanceClient.ConsoleClientApp\Dockerfile .
docker build -t wicharypawel/grpc-loadbalancing_greeter_server:latest -f .\NetCoreGrpc.LoadBalanceExternal.AspNetCoreServerApp\Dockerfile .
```

```
kubectl apply -f .\NetCoreGrpc.LoadBalanceExternal.k8s\greeter-server.yaml
kubectl create -f .\NetCoreGrpc.LoadBalanceClient.k8s\greeter-client-round-robin.yaml
```

```
kubectl logs greeter-client-round-robin
```

```
kubectl delete -f .\NetCoreGrpc.LoadBalanceClient.k8s\greeter-client-round-robin.yaml
kubectl delete -f .\NetCoreGrpc.LoadBalanceExternal.k8s\greeter-server.yaml
```
