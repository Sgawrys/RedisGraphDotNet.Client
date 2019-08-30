using System.Threading.Tasks;

namespace RedisGraphDotNet.Client
{
    public interface IRedisGraphClient
    {
        Task<bool> DeleteGraph(string graphName);
        Task<ResultSet> Query(string graphName, string query);
        Task<string> Explain(string graphName, string query);
    }
}