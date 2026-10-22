using System.Security.Cryptography;

namespace Contracts.Infrastructure.Security;

public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    public const string Algorithm = "PBKDF2-HMAC-SHA256";
    public const int Version = 1;
    private const int HashSize = 32;
    private const int SaltSize = 16;
    private readonly int iterations;
    private readonly PasswordDigest dummyDigest;

    public Pbkdf2PasswordHasher(PasswordPolicyOptions options)
    {
        if (options.Pbkdf2Iterations < 100_000) throw new InvalidOperationException("PBKDF2 iterations must be at least 100000.");
        iterations = options.Pbkdf2Iterations;
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        dummyDigest = new(Rfc2898DeriveBytes.Pbkdf2("dummy-verification", salt, iterations, HashAlgorithmName.SHA256, HashSize),
            salt, Algorithm, iterations, Version);
    }

    public PasswordDigest Hash(string password)
    {
        ArgumentNullException.ThrowIfNull(password);
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, HashSize);
        return new(hash, salt, Algorithm, iterations, Version);
    }

    public bool Verify(string password, PasswordDigest digest)
    {
        ArgumentNullException.ThrowIfNull(password);
        if (!string.Equals(digest.Algorithm, Algorithm, StringComparison.Ordinal) || digest.Version != Version ||
            digest.Iterations < 100_000 || digest.Salt.Length < SaltSize || digest.Hash.Length != HashSize) return false;
        byte[] actual = Rfc2898DeriveBytes.Pbkdf2(password, digest.Salt, digest.Iterations, HashAlgorithmName.SHA256, digest.Hash.Length);
        try { return CryptographicOperations.FixedTimeEquals(actual, digest.Hash); }
        finally { CryptographicOperations.ZeroMemory(actual); }
    }

    public void PerformDummyVerification(string password) => _ = Verify(password, dummyDigest);
}
