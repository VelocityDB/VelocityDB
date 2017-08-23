using System;

namespace VelocityGraph.Exceptions
{
  /// <summary>
  /// Exception thrown when a edge type id is specified and the edge type does not exist.
  /// </summary>
    [Serializable()]
    public class EdgeTypeDoesNotExistException : System.Exception
    {
      internal EdgeTypeDoesNotExistException() { }
      internal EdgeTypeDoesNotExistException(string message) : base(message) { }
      internal EdgeTypeDoesNotExistException(string message, System.Exception inner) : base(message, inner) { }

        // Constructor needed for serialization when exception propagates from a remoting server to the client.
      internal EdgeTypeDoesNotExistException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }
    }
}