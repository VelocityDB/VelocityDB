using System;

namespace VelocityGraph
{
  /// <summary>
  /// Exception thrown when a vertex id is specified for a vertex type and the vertex allready exist.
  /// </summary>
    [Serializable()]
    public class VertexAllreadyExistException : System.Exception
    {
      internal VertexAllreadyExistException() { }
      internal VertexAllreadyExistException(string message) : base(message) { }
      internal VertexAllreadyExistException(string message, System.Exception inner) : base(message, inner) { }

        // Constructor needed for serialization when exception propagates from a remoting server to the client.
      internal VertexAllreadyExistException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }
    }
}
