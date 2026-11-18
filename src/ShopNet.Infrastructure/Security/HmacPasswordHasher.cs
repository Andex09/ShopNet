using System.Security.Cryptography;
using System.Text;
using ShopNet.Application.Abstractions;

namespace ShopNet.Infrastructure.Security;

internal sealed class HmacPasswordHasher : IPasswordHasher
{
    public (byte[] Hash, byte[] Salt) HashPassword(string password)
    {
        using var hmac = new HMACSHA256();
        return (hmac.ComputeHash(Encoding.UTF8.GetBytes(password)), hmac.Key);
    }

    public bool VerifyPassword(string password, byte[] hash, byte[] salt)
    {
        using var hmac = new HMACSHA256(salt);
        var computed = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return CryptographicOperations.FixedTimeEquals(computed, hash);
    }
}
