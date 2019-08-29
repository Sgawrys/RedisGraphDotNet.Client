using System;

public class GraphDatabaseNameInUseException : Exception
{
    public GraphDatabaseNameInUseException(string message) : base(message) { }

    public GraphDatabaseNameInUseException(string message, Exception innerException) : base(message, innerException) { }
}