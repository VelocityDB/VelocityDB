using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VelocityGraph.Exceptions
{
  /// <summary>
  /// Exception thrown when the type of the tail vertex doesn't match the required tail VertexType of an EdgeType.
  /// </summary>
  [Serializable()]
  public class InvalidTailVertexTypeException : System.Exception
  {
    internal InvalidTailVertexTypeException() { }
    internal InvalidTailVertexTypeException(string message) : base(message) { }
    internal InvalidTailVertexTypeException(string message, System.Exception inner) : base(message, inner) { }

    // Constructor needed for serialization when exception propagates from a remoting server to the client.
    internal InvalidTailVertexTypeException(System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) { }
  }
}
