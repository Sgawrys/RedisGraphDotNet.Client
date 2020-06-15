using System.Threading.Tasks;
using StackExchange.Redis;

namespace RedisGraphDotNet.Client
{
    public class RedisGraphClient : IRedisGraphClient
    {
        private const string RedisGraphQueryCommand = "GRAPH.QUERY";
        private const string RedisGraphDeleteCommand = "GRAPH.DELETE";
        private const string RedisGraphExplainCommand = "GRAPH.EXPLAIN";

        private readonly ConnectionMultiplexer multiplexer;

        public RedisGraphClient(string address, int port) : this(ConnectionMultiplexer.Connect($"{address}:{port}")) { }

        public RedisGraphClient(ConnectionMultiplexer multiplexer)
        {
            this.multiplexer = multiplexer;
        }

        public async Task<ResultSet> Query(string graphName, string query)
        {
            var redisResult = await ExecuteQueryAsync(RedisGraphQueryCommand, graphName, query);
            return redisResult.AsResultSet();
        }

        public async Task<string> Explain(string graphName, string query)
        {
            var redisResult = await ExecuteQueryAsync(RedisGraphExplainCommand, graphName, query);
            return (string) redisResult;
        }

        public Task<bool> DeleteGraph(string graphName) {
            var db = multiplexer.GetDatabase();
            db.Execute(RedisGraphDeleteCommand, graphName);
            return Task.FromResult(true);
        }

        private Task<RedisResult> ExecuteQueryAsync(string commandName, string graphName, string query)
        {
            try
            {
                var db = multiplexer.GetDatabase();
                return db.ExecuteAsync(commandName, graphName, query);
            }
            catch (RedisServerException ex) when (ex.Message.Contains(RedisGraphErrorMessages.GraphDatabaseNotExists))
            {
                throw new GraphDatabaseNotExistException($"No graph database with name: {graphName} exists to perform query: {query} against.", ex);
            }
            catch (RedisServerException ex) when (ex.Message.Contains(RedisGraphErrorMessages.GraphDatabaseNameInUseByRedis))
            {
                throw new GraphDatabaseNameInUseException($"Graph database name: {graphName} is in use as a Redis key.", ex);
            }
        }
    }
}