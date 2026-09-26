using System.Security.Cryptography;
using System.Text;

namespace QdtCqts.Domain;

public static class SnapshotHash
{
    public static string Sha256(string value) => Sha256(Encoding.UTF8.GetBytes(value));

    public static string Sha256(byte[] bytes)
    {
        var digest = SHA256.HashData(bytes);
        return Convert.ToHexString(digest).ToLowerInvariant();
    }
}
