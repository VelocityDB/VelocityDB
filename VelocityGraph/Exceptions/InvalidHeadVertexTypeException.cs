using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VelocityGraph.Exceptions
{
  /// <summary>
  /// Exception thrown when the type of the head vertex doesn't match the required head VertexType of an EdgeType.
  /// </summary>
  [Serializable()]
  public class InvalidHeadVertexTypeException : System.Exception
  {
    internal InvalidHeadVertexTypeException() { }
    internal InvalidHeadVertexTypeException(string message) : base(message) { }
    internal InvalidHeadVertexTypeException(string message, System.Exception inner) : base(message, inner) { }

    // Constructor needed for serialization when exception propagates from a remoting server to the client.
    internal InvalidHeadVertexTypeException(System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) { }
  }
}

