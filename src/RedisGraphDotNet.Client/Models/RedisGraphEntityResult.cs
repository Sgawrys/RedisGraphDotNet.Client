using System.Collections.Generic;

public abstract class RedisGraphEntityResult : RedisGraphResult
{
    public int Id { get; private set; }

    public Dictionary<string, string> Properties { get; set; }

    public RedisGraphEntityResult(int id)
    {
        this.Id = id;
    }
}