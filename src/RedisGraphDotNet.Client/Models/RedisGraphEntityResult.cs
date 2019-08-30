using System.Collections.Generic;
using StackExchange.Redis;

public abstract class RedisGraphEntityResult : RedisGraphResult
{
    public int Id { get; private set; }

    public Dictionary<string, RedisValue> Properties { get; set; }

    public RedisGraphEntityResult(int id)
    {
        this.Id = id;
    }
}