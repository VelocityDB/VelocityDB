using System;

namespace VelocityGraph
{
  /// <summary>
  /// Exception thrown when a type id is specified for a property that does not exist.
  /// </summary>
    [Serializable()]
    public class InvalidPropertyIdException : System.Exception
    {
      internal InvalidPropertyIdException() { }
      internal InvalidPropertyIdException(string message) : base(message) { }
      internal InvalidPropertyIdException(string message, System.Exception inner) : base(message, inner) { }

      // Constructor needed for serialization when exception propagates from a remoting server to the client.
      internal InvalidPropertyIdException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }
    }
}
