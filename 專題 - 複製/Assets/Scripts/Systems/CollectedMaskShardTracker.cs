using System.Collections.Generic;

/// <summary>記錄已撿的血格碎片 ID（撿後物件銷毀，需靜態記帳）。</summary>
public static class CollectedMaskShardTracker
{
    static readonly HashSet<string> collected = new();

    public static bool IsCollected(string id) => collected.Contains(id);

    public static void MarkCollected(string id)
    {
        if (!string.IsNullOrEmpty(id)) collected.Add(id);
    }

    public static void Clear() => collected.Clear();

    public static List<string> ToList() => new(collected);

    public static void RestoreFrom(List<string> list)
    {
        collected.Clear();
        if (list != null)
            foreach (var id in list) collected.Add(id);
    }
}