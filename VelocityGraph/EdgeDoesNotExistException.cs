using System;

namespace VelocityGraph
{
  /// <summary>
  /// Exception thrown when a edge id is specified for an edge type and the edge does not exist.
  /// </summary>
    [Serializable()]
    public class EdgeDoesNotExistException : System.Exception
    {
      internal EdgeDoesNotExistException() { }
      internal EdgeDoesNotExistException(string message) : base(message) { }
      internal EdgeDoesNotExistException(string message, System.Exception inner) : base(message, inner) { }

        // Constructor needed for serialization when exception propagates from a remoting server to the client.
      internal EdgeDoesNotExistException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }
    }
}
