using System;

namespace VelocityGraph
{
  /// <summary>
  /// Exception thrown when a vertex type id is specified and the vertex type does not exist.
  /// </summary>
    [Serializable()]
    public class VertexTypeDoesNotExistException : System.Exception
    {
      internal VertexTypeDoesNotExistException() { }
      internal VertexTypeDoesNotExistException(string message) : base(message) { }
      internal VertexTypeDoesNotExistException(string message, System.Exception inner) : base(message, inner) { }

        // Constructor needed for serialization when exception propagates from a remoting server to the client.
      internal VertexTypeDoesNotExistException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }
    }
}