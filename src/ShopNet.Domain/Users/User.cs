using ShopNet.Domain.Common;

namespace ShopNet.Domain.Users;

public sealed class User
{
    public const int UserNameMaxLength = 50;

    private User()
    {
        UserName = string.Empty;
        Role = Roles.Basic;
        PasswordHash = [];
        PasswordSalt = [];
    }

    public int Id { get; private set; }

    /// <summary>Always stored lower-case and trimmed, so lookups are case-insensitive and can use the unique index.</summary>
    public string UserName { get; private set; }

    public string Role { get; private set; }

    public byte[] PasswordHash { get; private set; }

    public byte[] PasswordSalt { get; private set; }

    public static User Create(string userName, string role, byte[] passwordHash, byte[] passwordSalt)
    {
        var user = new User();
        user.Update(userName, role);
        user.SetPassword(passwordHash, passwordSalt);
        return user;
    }

    public static string NormalizeUserName(string userName) => userName.Trim().ToLowerInvariant();

    public void Update(string userName, string role)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new DomainException("User name is required.");
        }

        var normalized = NormalizeUserName(userName);
        if (normalized.Length > UserNameMaxLength)
        {
            throw new DomainException($"User name cannot exceed {UserNameMaxLength} characters.");
        }

        if (!Roles.IsValid(role))
        {
            throw new DomainException($"Unknown role '{role}'.");
        }

        UserName = normalized;
        Role = role;
    }

    public void SetPassword(byte[] passwordHash, byte[] passwordSalt)
    {
        ArgumentNullException.ThrowIfNull(passwordHash);
        ArgumentNullException.ThrowIfNull(passwordSalt);

        if (passwordHash.Length == 0 || passwordSalt.Length == 0)
        {
            throw new DomainException("Password hash and salt are required.");
        }

        PasswordHash = passwordHash;
        PasswordSalt = passwordSalt;
    }
}
