using System;

public class GraphDatabaseNotExistException : Exception
{
    public GraphDatabaseNotExistException(string message) : base(message) { }

    public GraphDatabaseNotExistException(string message, Exception innerException) : base(message, innerException) { }
}