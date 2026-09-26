using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using QdtCqts.Domain;

namespace QdtCqts.Calculation.Abstractions;

/// <summary>
/// Utilitário para cálculo de hash determinístico e canônico de entradas e saídas de regras e cálculos elétricos.
/// Garante que:
///   mesmos inputs + mesma versão + mesma regra = mesmo InputHash
/// </summary>
public static class DeterministicHashing
{
    public static string ComputeInputHash(IEnumerable<RuleInput> inputs)
    {
        var sorted = inputs
            .OrderBy(i => i.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var sb = new StringBuilder();
        for (int i = 0; i < sorted.Count; i++)
        {
            var item = sorted[i];
            if (i > 0)
            {
                sb.Append(';');
            }

            sb.Append(item.Name.Trim().ToUpperInvariant())
              .Append('=')
              .Append(FormatCanonicalValue(item.Value))
              .Append('[')
              .Append(item.Unit)
              .Append(']');
        }

        var canonicalBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return Convert.ToHexString(SHA256.HashData(canonicalBytes)).ToLowerInvariant();
    }

    public static string ComputeOutputHash(object? outputValue, UnitCode unit)
    {
        var canonical = $"{FormatCanonicalValue(outputValue)}[{unit}]";
        var bytes = Encoding.UTF8.GetBytes(canonical);
        return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }

    public static string FormatCanonicalValue(object? value)
    {
        if (value is null)
        {
            return "<null>";
        }

        return value switch
        {
            double d => d.ToString("G17", CultureInfo.InvariantCulture),
            float f => f.ToString("G9", CultureInfo.InvariantCulture),
            decimal m => m.ToString(CultureInfo.InvariantCulture),
            int i => i.ToString(CultureInfo.InvariantCulture),
            long l => l.ToString(CultureInfo.InvariantCulture),
            bool b => b ? "true" : "false",
            string s => $"\"{s.Trim()}\"",
            IEnumerable<double> doubles => "[" + string.Join(",", doubles.Select(d => d.ToString("G17", CultureInfo.InvariantCulture))) + "]",
            IEnumerable<object> objects => "[" + string.Join(",", objects.Select(FormatCanonicalValue)) + "]",
            _ => value.ToString()?.Trim() ?? "<null>"
        };
    }
}
