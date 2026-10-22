using System.Security.Cryptography;
using System.Text;
using System.Collections.Frozen;

namespace Contracts.Application.Security;

public sealed class PasswordPolicy(PasswordPolicyOptions options) : IPasswordPolicy
{
    private static readonly FrozenSet<string> BuiltInBannedHashes = new[]
    {
        "03AC674216F3E15C761EE1A5E255F067953623C8B388B4459E13F978D7C846F4",
        "5E884898DA28047151D0E56F8DC6292773603D0D6AABBDD62A11EF721D1542D8",
        "8C6976E5B5410415BDE908BD4DEE15DFB167A9C873FC4BB8A81F6F2AB448A918",
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public PasswordPolicyResult Validate(string password, string username)
    {
        ArgumentNullException.ThrowIfNull(password);
        List<string> errors = [];
        if (password.Length < options.MinimumLength) errors.Add($"Password must contain at least {options.MinimumLength} characters.");
        if (options.RequireUppercase && !password.Any(char.IsUpper)) errors.Add("Password must contain an uppercase character.");
        if (options.RequireLowercase && !password.Any(char.IsLower)) errors.Add("Password must contain a lowercase character.");
        if (options.RequireDigit && !password.Any(char.IsDigit)) errors.Add("Password must contain a digit.");
        if (options.RequireSymbol && !password.Any(character => !char.IsLetterOrDigit(character))) errors.Add("Password must contain a symbol.");
        if (!string.IsNullOrWhiteSpace(username) && password.Contains(username, StringComparison.OrdinalIgnoreCase)) errors.Add("Password must not contain the username.");
        string hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
        if (BuiltInBannedHashes.Contains(hash) || options.BannedPasswordSha256.Contains(hash, StringComparer.OrdinalIgnoreCase))
            errors.Add("Password is present in the configured breached/default-password blocklist.");
        return new(errors.Count == 0, errors);
    }
}
