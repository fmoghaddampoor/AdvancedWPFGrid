using System.Collections;
using System.Runtime.InteropServices;

namespace AdvancedWPFGrid.Data;

/// <summary>
/// A comparer that performs natural string sorting (e.g., "1" < "2" < "10").
/// </summary>
public class NaturalStringComparer : IComparer, IComparer<object?>
{
    [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
    private static extern int StrCmpLogicalW(string psz1, string psz2);

    private readonly bool _ascending;

    public NaturalStringComparer(bool ascending = true)
    {
        _ascending = ascending;
    }

    public int Compare(object? x, object? y)
    {
        string? s1 = x?.ToString();
        string? s2 = y?.ToString();

        if (s1 == null && s2 == null) return 0;
        if (s1 == null) return _ascending ? -1 : 1;
        if (s2 == null) return _ascending ? 1 : -1;

        int result = StrCmpLogicalW(s1, s2);
        return _ascending ? result : -result;
    }
}
