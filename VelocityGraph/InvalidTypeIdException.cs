using System;

namespace VelocityGraph
{
  /// <summary>
  /// Exception thrown when a type id is specified for a type that does not exist.
  /// </summary>
    [Serializable()]
    public class InvalidTypeIdException : System.Exception
    {
      internal InvalidTypeIdException() { }
      internal InvalidTypeIdException(string message) : base(message) { }
      internal InvalidTypeIdException(string message, System.Exception inner) : base(message, inner) { }

        // Constructor needed for serialization when exception propagates from a remoting server to the client.
      internal InvalidTypeIdException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }
    }
}
