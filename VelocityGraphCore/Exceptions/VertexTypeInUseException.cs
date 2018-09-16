using System;

namespace VelocityGraph.Exceptions
{
  /// <summary>
  /// Exception thrown when at least one vertex exist for a vertex type and attempting to remove the vertex type.
  /// </summary>
    [Serializable()]
    public class VertexTypeInUseException : System.Exception
    {
      internal VertexTypeInUseException() { }
      internal VertexTypeInUseException(string message) : base(message) { }
      internal VertexTypeInUseException(string message, System.Exception inner) : base(message, inner) { }

        // Constructor needed for serialization when exception propagates from a remoting server to the client.
      internal VertexTypeInUseException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }
    }
}

