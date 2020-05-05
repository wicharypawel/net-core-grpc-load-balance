# Net Core gRPC Load Balancing

# This repository

This repository present examples on how to work with gRPC load balancing.

Best links available online about gRPC load balancing are:
- https://grpc.io/blog/grpc-load-balancing/
- https://github.com/grpc/grpc/blob/master/doc/load-balancing.md
- https://www.youtube.com/watch?v=F2znfxn_5Hg

Scenarios covered by this repository are:
- round robin for gRPC C# client [README](scenarios/NetCore.CSharp.RoundRobin/README.md)
- lookaside load balancing for gRPC C# client [README](scenarios/NetCore.CSharp.Lookaside/README.md)
- round robin for gRPC dotnet client [README](scenarios/NetCore.DotNet.RoundRobin/README.md)
- lookaside load balancing for gRPC dotnet cient [README](scenarios/NetCore.DotNet.Lookaside/README.md)
- lookaside load balancing for gRPC dotnet cient with custom dns server (CoreDNS) [README](scenarios/NetCore.DotNet.Lookaside.CustomDns/README.md)
- lookaside load balancing for gRPC dotnet in `asp.net core` [README](scenarios/NetCore.DotNet.Lookaside.AspNetClient/README.md)
- sidecar load (static config) balancing for gRPC dotnet [README](scenarios/NetCore.DotNet.Sidecar.StaticConfiguration/README.md)
- sidecar load (dynamic config) balancing for gRPC dotnet [README](scenarios/NetCore.DotNet.Sidecar.DynamicConfiguration/README.md)
- istio service mesh load balancing for gRPC dotnet [README](scenarios/NetCore.DotNet.ServiceMesh/README.md)
- xDS balancing for gRPC dotnet [README](scenarios/NetCore.DotNet.xDS/README.md)
- lookaside load balancing for gRPC Java client [README](scenarios/Java.Lookaside/README.md)
- lookaside load balancing for gRPC Go client [README](scenarios/Go.Lookaside/README.md)
- lookaside load balancing for gRPC Python client [README](scenarios/Python.Lookaside/README.md) 

Repository also contains simple load balancer for gRPC in K8s written in C#

## Getting started

1. Download repository and move repository HEAD to latest release tag (eg. v0.6.0)  
2. Download .Net SDK (in the moment of writing 3.1.101)
3. Initialize git submodule(s) (see more `https://stackoverflow.com/questions/44366417/what-is-the-point-of-git-submodule-init`)
4. Set submodule repository HEAD to latest release tag (eg. v0.6.0) matching tag from step 1.
5. Open `README.md` file for scenario of choice
6. Setup K8s cluster
7. Create pods/services/deployments as described in `README.md` 

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