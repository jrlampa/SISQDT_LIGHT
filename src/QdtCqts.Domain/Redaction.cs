using System.Text.RegularExpressions;

namespace QdtCqts.Domain;

/// <summary>
/// Utilitário de higienização e proteção de dados sensíveis para logs e traces (Security First).
/// Mascara senhas, tokens, chaves de autenticação e dados cadastrais pessoais (PII).
/// </summary>
public static partial class SensitiveDataRedactor
{
    private static readonly Regex TokenRegex = new(
        @"(bearer\s+[a-zA-Z0-9_\-\.]+)|((api[-_]?key|token|secret|password|pwd)\s*[:=]\s*[""']?[^""'\s;,]+[""']?)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex ConnectionStringCredentialsRegex = new(
        @"(Password|Pwd|User\s*Id|Uid)\s*=\s*[^;]+",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex CpfRegex = new(
        @"\b\d{3}\.?\d{3}\.?\d{3}-?\d{2}\b",
        RegexOptions.Compiled);

    public static string Redact(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        var result = TokenRegex.Replace(input, match =>
        {
            var value = match.Value;
            var separatorIndex = value.IndexOfAny(new[] { ':', '=', ' ' });
            if (separatorIndex > 0)
            {
                var prefix = value[..separatorIndex];
                return $"{prefix}=***REDACTED***";
            }
            return "***REDACTED***";
        });

        result = ConnectionStringCredentialsRegex.Replace(result, "$1=***REDACTED***");
        result = CpfRegex.Replace(result, "***.***.***-**");

        return result;
    }
}
