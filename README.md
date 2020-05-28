# Net Core gRPC Load Balancing

# This repository

This repository present examples for a load balancing in gRPC for dotnet proposal.

Scenarios covered by this repository are:
- pick first for gRPC dotnet client [README](scenarios/NetCore.DotNet.PickFirst/README.md)
- round robin for gRPC dotnet client [README](scenarios/NetCore.DotNet.RoundRobin/README.md)

## Getting started

1. Download repository and switch to branch named `load-balancing-fork-examples`
2. Download .Net SDK (in the moment of writing I recommend having 3.1.101 & 3.1.201 & 3.1.300)
3. Initialize git submodule(s) (see more `https://stackoverflow.com/questions/44366417/what-is-the-point-of-git-submodule-init`)
4. Open `README.md` file for scenario of choice
5. Setup K8s cluster
6. Create pods/services/deployments as described in `README.md` 

## Sources

- https://grpc.io/blog/loadbalancing/ 
- https://github.com/grpc/grpc/blob/master/doc/load-balancing.md 
- https://docs.microsoft.com/en-us/dotnet/architecture/grpc-for-wcf-developers/load-balancing 
- https://github.com/grpc/grpc/blob/master/doc/load-balancing.md
- https://grpc.io/blog/grpc-load-balancing/
- https://github.com/jtattermusch/grpc-loadbalancing-kubernetes-examples/blob/master/grpc_loadbalancing_kubernetes_slides.pdf
- https://github.com/jtattermusch/grpc-loadbalancing-kubernetes-examples
- https://tools.ietf.org/html/rfc2782 - describe SRV records in DNS
- https://github.com/grpc/grpc/blob/master/doc/environment_variables.md
- https://kubernetes.io/blog/2018/11/07/grpc-load-balancing-on-kubernetes-without-tears/
- https://github.com/grpc/grpc/issues/19401
- https://github.com/grpc/grpc/pull/17723
- https://github.com/grpc/grpc/blob/master/doc/naming.md
- https://github.com/grpc/proposal/blob/master/A5-grpclb-in-dns.md
- https://kubernetes.io/docs/concepts/services-networking/dns-pod-service/
- https://kubernetes.io/docs/tasks/administer-cluster/dns-debugging-resolution/
- https://serverfault.com/questions/99483/nslookup-for-srv-records-or-any-non-a-records-in-non-interactive-mode
- https://github.com/grpc/grpc/blob/master/src/core/ext/filters/client_channel/resolver/dns/c_ares/dns_resolver_ares.cc
- https://github.com/grpc/grpc/blob/master/src/csharp/Grpc.Core/ChannelOptions.cs
- https://github.com/grpc/grpc/blob/master/include/grpc/impl/codegen/grpc_types.h
- https://github.com/grpc/grpc-dotnet/issues/521