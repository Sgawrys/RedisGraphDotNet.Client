using System;
using System.Collections.Generic;
using System.Linq;
using StackExchange.Redis;

internal static class GraphResultSetExtensions
{
    private const int NodeRecordResultLength = 3;
    private const int RelationRecordResultLength = 5;
    private const int QueryResultRecordLength = 3;

    internal static ResultSet AsResultSet(this RedisResult result)
    {
        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        var results = (RedisResult[])result;

        var resultSet = new ResultSet
        {
            Results = new Dictionary<string, List<RedisGraphResult>>(),
        };

        if (results.Length == QueryResultRecordLength)
        {
            var headerMetadata = (string[]) results[0];
            var records = (RedisResult[]) results[1];

            for (int i = 0; i < records.Length; i++)
            {
                var payload = (RedisResult[]) records[i];
                for (int j = 0; j < payload.Length; j++)
                {
                    if (!resultSet.Results.ContainsKey(headerMetadata[j])) {
                        resultSet.Results[headerMetadata[j]] = new List<RedisGraphResult>();
                    }

                    switch(payload[j].Type) 
                    {
                        case ResultType.MultiBulk:
                            resultSet.Results[headerMetadata[j]].Add(ParseGraphEntity((RedisResult[]) payload[j]));
                            break;
                        case ResultType.Integer:
                            resultSet.Results[headerMetadata[j]].Add(new ScalarResult<int>()
                            {
                                Value = (int) payload[j],
                                Type = ScalarResultType.Integer
                            });
                            continue;
                        case ResultType.BulkString:
                            resultSet.Results[headerMetadata[j]].Add(new ScalarResult<string>()
                            {
                                Value = (string) payload[j],
                                Type = ScalarResultType.String
                            });
                            break;
                        default:
                            break;
                    }
                }
            }
            
            resultSet.Metrics = ParseExecutionMetrics((RedisResult[]) results[2]);
        }
        else
        {
            resultSet.Metrics = ParseExecutionMetrics((RedisResult[]) results[0]);
        }

        return resultSet;
    }

    private static RedisGraphEntityResult ParseGraphEntity(RedisResult[] graphEntity)
    {
        if (graphEntity == null)
        {
            throw new ArgumentNullException(nameof(graphEntity));
        }

        switch(graphEntity.Length) {
            case NodeRecordResultLength:
                return graphEntity.AsGraphNode();
            case RelationRecordResultLength:
                return graphEntity.AsGraphRelation();
            default:
                throw new ArgumentException("Could not determine if graph node or relation.", nameof(graphEntity));
        }
    }

    private static QueryExecutionMetrics ParseExecutionMetrics(RedisResult[] contents)
    {
        var executionMetrics = new QueryExecutionMetrics();

        foreach (var entity in contents)
        {
            var kvp = ((string) entity).Split(':');

            var key = kvp[0];

            switch (key.ToLowerInvariant())
            {
                case "nodes created":
                    executionMetrics.NodesCreated = int.Parse(kvp[1].Trim());
                    break;
                case "nodes deleted":
                    executionMetrics.NodesDeleted = int.Parse(kvp[1].Trim());
                    break;
                case "properties set":
                    executionMetrics.PropertiesSet = int.Parse(kvp[1].Trim());
                    break;
                case "query internal execution time":
                    executionMetrics.ExecutionTimeInMilliseconds = ParseCommandExecutionTime(kvp[1]);
                    break;
                case "relationships created":
                    executionMetrics.RelationshipsCreated = int.Parse(kvp[1].Trim());
                    break;
                case "relationships deleted":
                    executionMetrics.RelationshipsDeleted = int.Parse(kvp[1].Trim());
                    break;
                case "labels added":
                    executionMetrics.LabelsCreated = int.Parse(kvp[1].Trim());
                    break;
                default:
                    break;
            }
        }

        return executionMetrics;
    }

    private static RedisGraphEntityResult AsGraphNode(this RedisResult[] redisResults)
    {
        if (redisResults == null)
        {
            throw new ArgumentNullException(nameof(redisResults));
        }

        if (redisResults.Length != NodeRecordResultLength)
        {
            throw new ArgumentException(nameof(redisResults));
        }

        var identifier = ((RedisResult[])redisResults[0])[1];
        var labels = (string[])((RedisResult[])redisResults[1])[1];
        var propertyKeys = (string[])((RedisResult[])redisResults[2])[0];

        var node = new Node((int)identifier)
        {
            Labels = labels.ToList(),
            Properties = new Dictionary<string, string>(),
        };

        return node;
    }

    private static RedisGraphEntityResult AsGraphRelation(this RedisResult[] redisResults)
    {
        if (redisResults == null)
        {
            throw new ArgumentNullException(nameof(redisResults));
        }

        if (redisResults.Length != RelationRecordResultLength)
        {
            throw new ArgumentException(nameof(redisResults));
        }

        var identifier = ((RedisResult[]) redisResults[0])[1];
        var relationshipType = (string) ((RedisResult[]) redisResults[1])[1];
        var sourceNodeIdentifier = (int) ((RedisResult[]) redisResults[2])[1];
        var destinationNodeIdentifier = (int) ((RedisResult[]) redisResults[3])[1];

        return new Relation((int) identifier)
        {
            Type = relationshipType,
            SourceNodeId = sourceNodeIdentifier,
            DestinationNodeId = destinationNodeIdentifier,
            Properties = new Dictionary<string, string>(),
        };
    }

    private static double ParseCommandExecutionTime(string executionTimeString)
    {
        return double.Parse(executionTimeString.Split(new string[] { "milliseconds"}, StringSplitOptions.RemoveEmptyEntries)[0]);
    }
}