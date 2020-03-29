# Private notes and todo list

Skip this file. This are private notes.

## DOCS:
- https://github.com/grpc/grpc/blob/master/doc/load-balancing.md
- https://github.com/grpc/grpc/blob/master/doc/naming.md
- https://github.com/grpc/grpc/blob/master/doc/service_config.md
- https://github.com/grpc/proposal/blob/master/A5-grpclb-in-dns.md
- https://github.com/grpc/proposal/blob/master/A2-service-configs-in-dns.md
- https://github.com/grpc/proposal/blob/master/A24-lb-policy-config.md
https://github.com/grpc/grpc-proto/blob/master/grpc/service_config/service_config.proto
- https://github.com/grpc/proposal/blob/master/A10-avoid-grpclb-and-service-config-for-localhost-and-ip-literals.md
- https://github.com/grpc/proposal/blob/master/A26-grpclb-selection.md
- https://github.com/grpc/grpc-dotnet/issues/521

## Key design decisions for Grpc.Net.Client

- load balancer is connected via unsecure channel
  - based on docs https://github.com/grpc/grpc/blob/master/doc/load-balancing.md
  - based on implementations for other languages
- override authority is done via grpcOptions class instead of `dns://authorityIp:authorityPort/target` 
  - reson is to reduce code complexity
- `dns://` scheme is not default for Grpc.Net.Client
  - according to naming docs (https://github.com/grpc/grpc/blob/master/doc/naming.md) default scheme should be `dns://` 
  - however in grpc for dotnet there was no default. Moreover `https://` is recommended by official docs

## Load balancer todo list

- remove hardcoded service name it should be presented by client
- ensure lb picks up changing configuration from k8s
- ensure grpclb use bi-directional streaming correclty in parallel (can but it is not a must) [method CreateSubChannelsAsync]
- secure connection support
- remove hardcoded values
- integrate with consul or etcd

## Load balancer features
- integrated with 4 languages
- integrated with both grpc C# client and grpc dotnet client
- allow report load balance of servers (thing about this opaque token for service)
- service expose service config

## About TXT records

Multiple LB policies can be specified; clients will iterate through
the list in order and stop at the first policy that they support. If none
are supported, the service config is considered invalid.