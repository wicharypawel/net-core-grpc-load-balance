Execute in root directory

```
docker build -t wicharypawel/grpc-loadbalancing-demo2018_greeter_client:my8 .\NetCoreGrpc.LoadBalanceExternal.ConsoleClientApp\
docker build -t wicharypawel/grpc-loadbalancing-demo2018_greeter_server_balancer:my8 -f .\NetCoreGrpc.LoadBalanceExternal.GrpcLoadBalancer\Dockerfile .
docker build -t wicharypawel/grpc-loadbalancing-demo2018_greeter_server:my8 .\NetCoreGrpc.LoadBalanceExternal.AspNetCoreServerApp\
```

```
kubectl apply -f .\NetCoreGrpc.LoadBalanceExternal.k8s\greeter-server.yaml
kubectl apply -f .\NetCoreGrpc.LoadBalanceExternal.k8s\greeter-server-balancer.yaml
kubectl create -f .\NetCoreGrpc.LoadBalanceExternal.k8s\greeter-client-lookaside-lb.yaml
```

```
kubectl logs greeter-client-lookaside-lb
```

```
kubectl delete -f .\NetCoreGrpc.LoadBalanceExternal.k8s\greeter-client-lookaside-lb.yaml
kubectl delete -f .\NetCoreGrpc.LoadBalanceExternal.k8s\greeter-server-balancer.yaml
kubectl delete -f .\NetCoreGrpc.LoadBalanceExternal.k8s\greeter-server.yaml
```


