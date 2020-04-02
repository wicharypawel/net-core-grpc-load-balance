namespace SimpleBalancer.Services.Abstraction
{
    public sealed class EndpointEntry
    {
        public EndpointEntry(string ip, int port)
        {
            Ip = ip;
            Port = port;
        }

        public string Ip { get; }
        public int Port { get; }
    }
}
