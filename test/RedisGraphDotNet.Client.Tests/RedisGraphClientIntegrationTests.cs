using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RedisGraphDotNet.Client.Tests
{
    public class RedisGraphClientIntegrationTests : IDisposable
    {
        private readonly string TestGraphName = "test";

        private IRedisGraphClient redisGraphClient { get; set; }

        public RedisGraphClientIntegrationTests()
        {
            redisGraphClient = new RedisGraphClient("localhost", 6379, TestGraphName);
        }

        [Fact]
        public async Task CreateSingularNodeInGraph()
        {
            var result = await redisGraphClient.Query(TestGraphName, "CREATE (:node1 {testName: \"testValue\"})");

            Assert.NotNull(result);
            Assert.Equal(1, result.Metrics.NodesCreated);
            Assert.Equal(1, result.Metrics.PropertiesSet);
        }

        [Fact]
        public async Task CreateSingularNodeWithoutProperties()
        {
            var result = await redisGraphClient.Query(TestGraphName, "CREATE (:node1)");

            Assert.NotNull(result);
            Assert.Equal(1, result.Metrics.NodesCreated);
            Assert.Equal(0, result.Metrics.PropertiesSet);
        }

        [Fact]
        public async Task CreateMultipleNodesInGraph()
        {
            var result = await redisGraphClient.Query(TestGraphName, "CREATE (:node1 { prop1: 1}), (:node2 { prop2: 2})");

            Assert.NotNull(result);
            Assert.Equal(2, result.Metrics.NodesCreated);
            Assert.Equal(2, result.Metrics.PropertiesSet);
        }

        [Fact]
        public async Task CreatesNodesWithRelationship()
        {
            var result = await redisGraphClient.Query(TestGraphName, "CREATE (:node1 { prop1: 1 })-[:parent]->(:node2 { prop2: 2})");

            Assert.NotNull(result);
            Assert.Equal(2, result.Metrics.NodesCreated);
            Assert.Equal(2, result.Metrics.PropertiesSet);
            Assert.Equal(1, result.Metrics.RelationshipsCreated);
            Assert.Equal(2, result.Metrics.LabelsCreated);
        }

        [Fact]
        public async Task CreateNodeAndRetrieveNode()
        {
            var createResult = await redisGraphClient.Query(TestGraphName, "CREATE (:node1)");

            var queryResult = await redisGraphClient.Query(TestGraphName, "MATCH (a:node1) RETURN a");

            Assert.NotNull(createResult);
            Assert.NotNull(queryResult);
            Assert.Equal(1, createResult.Metrics.NodesCreated);
            Assert.Single(queryResult.Results);
        }

        [Fact]
        public async Task CreateNodesWithRelationshipAndRetrieveNode()
        {
            var createResult = await redisGraphClient.Query(TestGraphName, "CREATE (:node1 { prop1: 1 })-[:parent]->(:node2 { prop2: 2})");

            var queryResult = await redisGraphClient.Query(TestGraphName, "MATCH (a:node1)-[:parent]->(:node2 { prop2: 2}) RETURN a");

            Assert.NotNull(createResult);
            Assert.NotNull(queryResult);
            Assert.Equal(2, createResult.Metrics.NodesCreated);
            Assert.Single(queryResult.Results);
        }

        [Fact]
        public async Task QueryOnRelationshipFilterForNodeWhichDoesNotExist()
        {
            var createResult = await redisGraphClient.Query(TestGraphName, "CREATE (:node1 { prop1: 1 })-[:parent]->(:node2 { prop2: 2})");

            var queryResult = await redisGraphClient.Query(TestGraphName, "MATCH (a:node1)-[:parent]->(:node2 { prop2: 1}) RETURN a");

            Assert.NotNull(createResult);
            Assert.NotNull(queryResult);
            Assert.Equal(2, createResult.Metrics.NodesCreated);
            Assert.Empty(queryResult.Results);
        }

        [Fact]
        public async Task QueryReturnScalarValue()
        {
            var createResult = await redisGraphClient.Query(TestGraphName, "CREATE (:node1 { prop1: 1 })");

            var queryResult = await redisGraphClient.Query(TestGraphName, "MATCH (a:node1) WHERE a.prop1 = 1 RETURN a.prop1");

            Assert.NotNull(createResult);
            Assert.NotNull(queryResult);
            Assert.Equal(1, createResult.Metrics.NodesCreated);
            Assert.Single(queryResult.Results);
        }

        [Fact]
        public async Task QueryReturnsMultipleNodes()
        {
            var createResult = await redisGraphClient.Query(TestGraphName, "CREATE (:node1 { prop1: 1 }),(:node1 { prop1: 1})");

            var queryResult = await redisGraphClient.Query(TestGraphName, "MATCH (a:node1) WHERE a.prop1 = 1 RETURN a");

            Assert.NotNull(createResult);
            Assert.NotNull(queryResult);
            Assert.Equal(2, createResult.Metrics.NodesCreated);
            Assert.Equal(1, queryResult.Results.Count);
            Assert.True(queryResult.Results.ContainsKey("a"));
        }

        [Fact]
        public async Task QueryReturnsMultipleNodesAndScalarValues()
        {
            var createResult = await redisGraphClient.Query(TestGraphName, "CREATE (:node1 { prop1: 1 }),(:node1 { prop1: 1})");

            var queryResult = await redisGraphClient.Query(TestGraphName, "MATCH (a:node1) WHERE a.prop1 = 1 RETURN a.prop1,a");

            Assert.NotNull(createResult);
            Assert.NotNull(queryResult);
            Assert.Equal(2, createResult.Metrics.NodesCreated);
            Assert.Equal(2, queryResult.Results.Count);
            Assert.True(queryResult.Results.ContainsKey("a.prop1"));
            Assert.True(queryResult.Results.ContainsKey("a"));
        }

        [Fact]
        public async Task QueryReturnsRelation()
        {
            var createResult = await redisGraphClient.Query(TestGraphName, "CREATE (:node1)-[:parent]->(:node2)");

            var queryResult = await redisGraphClient.Query(TestGraphName, "MATCH (:node1)-[a:parent]->(:node2) RETURN a");

            Assert.NotNull(createResult);
            Assert.NotNull(queryResult);
            Assert.Equal(2, createResult.Metrics.NodesCreated);
            Assert.Equal(1, createResult.Metrics.RelationshipsCreated);
            Assert.Single(queryResult.Results);
            Assert.Single(queryResult.Results.First().Value);
        }

        [Fact]
        public async Task TestGraphExplainExecutionPlan()
        {
            var createGraphResult = await redisGraphClient.Query(TestGraphName, "CREATE (:node2)");
            var explainResult = await redisGraphClient.Explain(TestGraphName, "CREATE (:node1)");
            var queryResult = await redisGraphClient.Query(TestGraphName, "MATCH (a:node1) RETURN a");

            Assert.NotNull(explainResult);
            Assert.NotNull(queryResult);
            Assert.Empty(queryResult.Results);
        }

        [Fact]
        public async Task MatchAndCreateSingularNodeQuery()
        {
            var createFirstNode = await redisGraphClient.Query(TestGraphName, "CREATE (:node1)");
            var createSecondNode = await redisGraphClient.Query(TestGraphName, "MATCH (a:node1) CREATE (a)-[:parent]->(:node2) RETURN a");

            Assert.NotNull(createFirstNode);
            Assert.NotNull(createSecondNode);
        }

        [Fact]
        public async Task CallLabelsProcedure()
        {
            var createFirstNode = await redisGraphClient.Query(TestGraphName, "CREATE (:node1)");
            var callProcedure = await redisGraphClient.Query(TestGraphName, "CALL db.labels()");

            Assert.NotNull(createFirstNode);
            Assert.NotNull(callProcedure);
            Assert.Equal(1, createFirstNode.Metrics.NodesCreated);
            Assert.Single(callProcedure.Results);
        }

        [Fact]
        public async Task CreateRelationWithProperties()
        {
            var createQuery = await redisGraphClient.Query(TestGraphName, "CREATE (:node1)-[:parent {prop1: 1}]->(:node2)");
            var relationshipQuery = await redisGraphClient.Query(TestGraphName, "MATCH (:node1)-[a:parent]->(:node2) RETURN a");

            Assert.NotNull(createQuery);
            Assert.Equal(1, createQuery.Metrics.PropertiesSet);
            Assert.Equal(2, createQuery.Metrics.NodesCreated);
            Assert.Equal(1, createQuery.Metrics.RelationshipsCreated);
            Assert.NotNull(relationshipQuery);
            Assert.Single(relationshipQuery.Results);
        }

        public void Dispose()
        {
            redisGraphClient.DeleteGraph(TestGraphName);
        }
    }
}