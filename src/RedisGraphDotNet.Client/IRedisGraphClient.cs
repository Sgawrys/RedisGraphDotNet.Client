using System.Threading.Tasks;

namespace RedisGraph.Client
{
    public interface IRedisGraphClient
    {
        Task<bool> DeleteGraph(string graphName);
        Task<ResultSet> Query(string graphName, string query);
        Task<string> Explain(string graphName, string query);
    }
}