public class Relation : RedisGraphEntityResult
{
    public string Type { get; set; }

    public int SourceNodeId { get; set; }

    public int DestinationNodeId { get; set; }

    public Relation(int id) : base(id) { }
}