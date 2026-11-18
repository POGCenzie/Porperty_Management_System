namespace PropertyManagementSystem.Configuration;

internal static class DashboardPreferences
{
    private static readonly string PreferencesDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Syrento",
        "DashboardPreferences");

    internal static IReadOnlySet<string> LoadAttentionItems(
        int userId,
        IReadOnlySet<string> validKeys,
        IEnumerable<string> defaultKeys)
    {
        var path = GetAttentionPath(userId);
        try
        {
            if (!File.Exists(path))
            {
                return defaultKeys.ToHashSet(StringComparer.OrdinalIgnoreCase);
            }

            return File.ReadAllLines(path)
                .Select(item => item.Trim())
                .Where(item => validKeys.Contains(item))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return defaultKeys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
    }

    internal static void SaveAttentionItems(int userId, IEnumerable<string> selectedKeys)
    {
        try
        {
            Directory.CreateDirectory(PreferencesDirectory);
            File.WriteAllLines(
                GetAttentionPath(userId),
                selectedKeys.OrderBy(item => item, StringComparer.OrdinalIgnoreCase));
        }
        catch
        {
            // Keep the current in-memory selection if local preferences cannot be written.
        }
    }

    internal static IReadOnlySet<string> LoadTableSelection(
        int userId,
        string tableKey,
        string selectionType,
        IReadOnlySet<string> validKeys,
        IEnumerable<string> defaultKeys)
    {
        var path = GetTableSelectionPath(userId, tableKey, selectionType);
        try
        {
            if (!File.Exists(path))
            {
                return defaultKeys.ToHashSet(StringComparer.OrdinalIgnoreCase);
            }

            var savedKeys = File.ReadAllLines(path)
                .Select(item => item.Trim())
                .Where(item => validKeys.Contains(item))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            return savedKeys.Count > 0
                ? savedKeys
                : defaultKeys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return defaultKeys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
    }

    internal static void SaveTableSelection(
        int userId,
        string tableKey,
        string selectionType,
        IEnumerable<string> selectedKeys)
    {
        try
        {
            Directory.CreateDirectory(PreferencesDirectory);
            File.WriteAllLines(
                GetTableSelectionPath(userId, tableKey, selectionType),
                selectedKeys.OrderBy(item => item, StringComparer.OrdinalIgnoreCase));
        }
        catch
        {
            // Keep the current in-memory selection if local preferences cannot be written.
        }
    }

    private static string GetAttentionPath(int userId) =>
        Path.Combine(PreferencesDirectory, $"needs-attention-user-{userId}.txt");

    private static string GetTableSelectionPath(
        int userId,
        string tableKey,
        string selectionType)
    {
        var safeTableKey = string.Concat(tableKey.Where(character =>
            char.IsLetterOrDigit(character) || character is '-' or '_'));
        var safeSelectionType = string.Concat(selectionType.Where(character =>
            char.IsLetterOrDigit(character) || character is '-' or '_'));
        return Path.Combine(
            PreferencesDirectory,
            $"table-{safeTableKey}-{safeSelectionType}-user-{userId}.txt");
    }
}
