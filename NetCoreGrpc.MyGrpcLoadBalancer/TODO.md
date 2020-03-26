Load balancer todo list


- remove hardcoded service name it should be presented by client
- ensure lb picks up changing configuration from k8s
- ensure grpclb use bi-directional streaming correclty in parallel (can but it is not a must) [method CreateSubChannelsAsync]
- secure connection support
- remove hardcoded values

Load balancer features
- integrated with 4 languages
- integrated with both grpc C# client and grpc dotnet client
- allow report load balance of servers (thing about this opaque token for service)
- service expose service config
- integrate with consul???

Grpc.Net.Client TODO list
- implement pick_first policy [DONE]
- implement round_robin policy [DONE]
- implement grpclb policy [DONE]
- merge into one project [REJECTED] - because it will put extra dependencies on main project 
- support for secure connection during load balancing [DONE]
- split lb policy and dns resolver [DONE]
- refactoring [DONE]
- enable logging for policies [DONE]
- add some validation logic for policies [DONE]
- allow dns scheme (dns://) as prefix
- enable secure connection for dns scheme [DONE]
- allow override dns server for DnsClientNameResolver [DONE]
- allow searching for service config in TXT using `_grpc_config` prefix [DONE]
- validate schemes depending on resolver [DONE]
- rename NameResolver to ResolverPlugin [DONE]
- add static ResolverPlugin / hardcoded dns resolution [DONE] 
- DODAĆ xDS policy [REJECTED] - because it is experimental
- integracja dla 

Key decisions
- load balancer is connected via unsecure channel https://github.com/grpc/grpc/blob/master/doc/load-balancing.md
- override authority is done via options instead of `dns://authorityIp:authorityPort/target` to reduce complexity
- according to docs default scheme is `dns://` however in grpc for dotnet there is no default. Moreover `https://` is recommended by official docs.


NEXT:
- https://github.com/grpc/grpc/blob/master/doc/load-balancing.md
- https://github.com/grpc/grpc/blob/master/doc/naming.md
- https://github.com/grpc/grpc/blob/master/doc/service_config.md
- https://github.com/grpc/proposal/blob/master/A5-grpclb-in-dns.md
- https://github.com/grpc/proposal/blob/master/A2-service-configs-in-dns.md (HERE ABOUT TXT)
- https://github.com/grpc/proposal/blob/master/A24-lb-policy-config.md
https://github.com/grpc/grpc-proto/blob/master/grpc/service_config/service_config.proto
- https://github.com/grpc/proposal/blob/master/A10-avoid-grpclb-and-service-config-for-localhost-and-ip-literals.md
- https://github.com/grpc/grpc-dotnet/issues/521




TXT:
```

Multiple LB policies can be specified; clients will iterate through
the list in order and stop at the first policy that they support. If none
are supported, the service config is considered invalid.


```