namespace QdtCqts.Domain;

public static class TopologyExtensions
{
    public static int IndexOf(this IReadOnlyList<string> values, string value) =>
        Enumerable.Range(0, values.Count).FirstOrDefault(index => string.Equals(values[index], value, StringComparison.Ordinal));
}
