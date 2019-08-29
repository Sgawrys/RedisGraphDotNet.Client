public class ScalarResult<T> : RedisGraphResult
{
    public ScalarResultType Type { get; set; }

    public T Value { get; set; }
}

public enum ScalarResultType
{
    Integer,
    Null,
    String,
    Boolean,
    Double,
}