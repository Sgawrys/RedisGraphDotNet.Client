# RedisGraphDotNet.Client

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Build Status](https://dev.azure.com/stefangawrys0820/stefangawrys/_apis/build/status/Sgawrys.RedisGraphDotNet.Client?branchName=master)](https://dev.azure.com/stefangawrys0820/stefangawrys/_build/latest?definitionId=1&branchName=master)
![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/RedisGraphDotNet.Client)

A C# client library for [RedisGraph](https://oss.redislabs.com/redisgraph/).

## How to use

Add the client library NuGet package to your project:

`dotnet add package RedisGraphDotNet.Client`

## Examples

### Initializing the client

Initialize the client by specifying Redis server address and port:

```csharp
var redisGraphClient = new RedisGraphClient("localhost", 6739);
```

Initialize the client by providing a [ConnectionMultiplexer](https://stackexchange.github.io/StackExchange.Redis/Basics.html):

```csharp
var connectionMultiplexer = ConnectionMultiplexer.Connect("localhost", 6739);

// Other code here.

var redisGraphClient = new RedisGraphClient(connectionMultiplexer);
```

### Creating nodes

Create a singular node:

```csharp
redisGraphClient.Query("myTestGraphDatabase", "CREATE (:myTestNode)");
```

Create a node with properties:

```csharp
redisGraphClient.Query("myTestGraphDatabase", "CREATE (:myTestNode { myTestProperty: 1 })");
```

Create multiple nodes:

```csharp
redisGraphClient.Query("myTestGraphDatabase", "CREATE (a:myTestNode),(b:myTestNode)");
```

Create a node with a relationship to another node:

```csharp
// Creates two nodes with label 'myTestNode' with a 'parent' relationship type.
redisGraphClient.Query("myTestGraphDatabase", "CREATE (a:myTestNode)-[:parent]->(b:myTestNode)");
```

### Querying for nodes, relations, and properties

Retrieve node(s) with label `myTestNode`:

```csharp
redisGraphClient.Query("myTestGraphDatabase", "MATCH (a:myTestNode) RETURN a");
```

Retrieve node(s) with a `parent` relationship type:

```csharp
redisGraphClient.Query("myTestGraphDatabase", "MATCH (a)->[:parent]->(b) RETURN a,b");
```

### Deleting the graph database

```csharp
redisGraphClient.DeleteGraph("myTestGraphDatabase");
```

## Running Tests

Run tests with the following command:

`dotnet test`

The tests currently expect Redis to be running locally on 6379.
