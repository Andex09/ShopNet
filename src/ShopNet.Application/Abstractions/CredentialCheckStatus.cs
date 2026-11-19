namespace ShopNet.Application.Abstractions;

public enum CredentialCheckStatus
{
    Succeeded,
    InvalidCredentials,
    LockedOut,
}
