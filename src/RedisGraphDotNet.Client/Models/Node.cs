using System.Collections.Generic;

public class Node : RedisGraphEntityResult
{
    public List<string> Labels { get; set; }

    public Node(int id) : base(id) { }
}