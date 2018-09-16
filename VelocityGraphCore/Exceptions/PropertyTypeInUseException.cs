using System;

namespace VelocityGraph.Exceptions
{
  /// <summary>
  /// Exception thrown when at least one vertex or edge uses a property type when attempting to remove the property type
  /// </summary>
    [Serializable()]
    public class PropertyTypeInUseException : System.Exception
    {
      internal PropertyTypeInUseException() { }
      internal PropertyTypeInUseException(string message) : base(message) { }
      internal PropertyTypeInUseException(string message, System.Exception inner) : base(message, inner) { }

        // Constructor needed for serialization when exception propagates from a remoting server to the client.
      internal PropertyTypeInUseException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }
    }
}
