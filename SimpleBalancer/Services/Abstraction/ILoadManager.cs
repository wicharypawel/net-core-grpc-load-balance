namespace SimpleBalancer.Services.Abstraction
{
    public interface ILoadManager
    {
        public string GetLoadBalanceToken(string serverAddress);
    }
}
