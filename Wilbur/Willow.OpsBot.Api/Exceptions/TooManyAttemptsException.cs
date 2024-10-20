using System.Runtime.Serialization;

namespace Willow.OpsBot.Api.Exceptions;

[Serializable]
public class TooManyAttemptsException : Exception
{
    public TooManyAttemptsException(string? message) : base(message)
    {
    }

    protected TooManyAttemptsException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
