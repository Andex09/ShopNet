namespace ShopNet.Application.Common;

/// <summary>The requested resource does not exist. Mapped to HTTP 404 by the API.</summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string resource, object key)
        : base($"{resource} '{key}' was not found.")
    {
    }

    public NotFoundException() { }

    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>The request conflicts with the current state. Mapped to HTTP 409 by the API.</summary>
public sealed class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }

    public ConflictException() { }

    public ConflictException(string message, Exception innerException) : base(message, innerException) { }
}
