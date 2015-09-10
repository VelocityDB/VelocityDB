using System;

namespace VelocityGraph
{
  /// <summary>
  /// Exception thrown when at least one edge exist for a edge type and attempting to remove the edge type.
  /// </summary>
    [Serializable()]
    public class EdgeTypeInUseException : System.Exception
    {
      internal EdgeTypeInUseException() { }
      internal EdgeTypeInUseException(string message) : base(message) { }
      internal EdgeTypeInUseException(string message, System.Exception inner) : base(message, inner) { }

        // Constructor needed for serialization when exception propagates from a remoting server to the client.
      internal EdgeTypeInUseException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }
    }
}