using System;

namespace VelocityGraph.Exceptions
{
  /// <summary>
  /// Exception thrown when a vertex id is specified for a vertex type and the vertex does not exist.
  /// </summary>
    [Serializable()]
    public class VertexDoesNotExistException : System.Exception
    {
      internal VertexDoesNotExistException() { }
      internal VertexDoesNotExistException(string message) : base(message) { }
      internal VertexDoesNotExistException(string message, System.Exception inner) : base(message, inner) { }

        // Constructor needed for serialization when exception propagates from a remoting server to the client.
      internal VertexDoesNotExistException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }
    }
}

