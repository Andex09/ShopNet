namespace ShopNet.Domain.Common;

/// <summary>A business rule was violated. Mapped to HTTP 400 by the API.</summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }

    public DomainException() { }

    public DomainException(string message, Exception innerException) : base(message, innerException) { }
}
