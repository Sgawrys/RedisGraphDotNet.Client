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
        private readonly string graphName;

        public RedisGraphClient(string address, int port) : this(ConnectionMultiplexer.Connect($"{address}:{port}")) { }

        public RedisGraphClient(string address, int port, string graphName) : this(address, port)
        {
            this.graphName = graphName;
        }

        public RedisGraphClient(ConnectionMultiplexer multiplexer)
        {
            this.multiplexer = multiplexer;
        }

        public Task<ResultSet> Query(string graphName, string query)
        {
            return Task.FromResult(ExecuteQuery(RedisGraphQueryCommand, graphName, query).AsResultSet());
        }

        public Task<string> Explain(string graphName, string query)
        {
            return Task.FromResult((string) ExecuteQuery(RedisGraphExplainCommand, graphName, query));
        }

        public Task<bool> DeleteGraph(string graphName) {
            var db = multiplexer.GetDatabase();
            db.Execute(RedisGraphDeleteCommand, graphName);
            return Task.FromResult(true);
        }

        private RedisResult ExecuteQuery(string commandName, string graphName, string query)
        {
            try
            {
                var db = multiplexer.GetDatabase();
                return db.Execute(commandName, graphName, query);
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