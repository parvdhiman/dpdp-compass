using System.Text;

namespace DPDP.Application.Common.Csv;

/// <summary>
/// Minimal RFC 4180 CSV writer — no external dependency for something
/// this small. Used by the Module 9 export endpoints
/// (docs/DATA_INVENTORY.md); reusable by any future export.
/// </summary>
public static class CsvWriter
{
    public static string Write(IReadOnlyList<string> headers, IEnumerable<IReadOnlyList<string?>> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(',', headers.Select(Escape)));
        foreach (var row in rows)
        {
            sb.AppendLine(string.Join(',', row.Select(Escape)));
        }

        return sb.ToString();
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var needsQuoting = value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
        var escaped = value.Replace("\"", "\"\"");
        return needsQuoting ? $"\"{escaped}\"" : escaped;
    }
}
