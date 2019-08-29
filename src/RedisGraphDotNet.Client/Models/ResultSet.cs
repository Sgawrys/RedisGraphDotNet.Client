using System.Collections.Generic;

public class ResultSet
{
    public Dictionary<string, List<RedisGraphResult>> Results { get; set; }

    public QueryExecutionMetrics Metrics { get; set; }
}