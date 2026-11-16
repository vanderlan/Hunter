using System;
using System.Security.Cryptography;

namespace GeminiCode.Hunter.Domain.Core.Helpers;

/// <summary>
/// Generates unguessable, URL-safe identifiers for anything exposed on public pages.
/// </summary>
public static class SecureTokenGenerator
{
    private const string Alphabet = "abcdefghijkmnpqrstuvwxyz23456789"; // no 0/o/1/l ambiguity

    /// <summary>Short human-friendly code, e.g. for job position URLs. 10 chars ≈ 50 bits.</summary>
    public static string NewPublicCode(int length = 10)
    {
        var buffer = new char[length];
        for (var i = 0; i < length; i++)
        {
            buffer[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];
        }
        return new string(buffer);
    }

    /// <summary>Long token for candidate sessions and email confirmation links. 32 bytes ≈ 256 bits.</summary>
    public static string NewToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}
