namespace PropertyManagementSystem;

internal static class AppTheme
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Syrento",
        "theme.v2.mode");

    static AppTheme()
    {
        try
        {
            IsDark = File.Exists(SettingsPath) &&
                string.Equals(File.ReadAllText(SettingsPath).Trim(), "dark", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            IsDark = false;
        }
    }

    internal static bool IsDark { get; private set; }

    internal static Color PrimaryBlue => IsDark ? Color.FromArgb(39, 174, 255) : Color.FromArgb(13, 124, 242);
    internal static Color PrimaryBlueHover => IsDark ? Color.FromArgb(19, 145, 226) : Color.FromArgb(5, 87, 203);
    internal static Color PageBackground => IsDark ? Color.FromArgb(8, 25, 62) : Color.FromArgb(232, 245, 255);
    internal static Color Surface => IsDark ? Color.FromArgb(13, 35, 76) : Color.FromArgb(252, 254, 255);
    internal static Color ElevatedSurface => IsDark ? Color.FromArgb(18, 44, 91) : Color.White;
    internal static Color InputBackground => IsDark ? Color.FromArgb(11, 31, 68) : Color.FromArgb(248, 252, 255);
    internal static Color Border => IsDark ? Color.FromArgb(49, 82, 135) : Color.FromArgb(193, 216, 242);
    internal static Color TextPrimary => IsDark ? Color.FromArgb(244, 249, 255) : Color.FromArgb(5, 35, 91);
    internal static Color TextSecondary => IsDark ? Color.FromArgb(166, 191, 225) : Color.FromArgb(79, 108, 153);
    internal static Color Sidebar => Color.FromArgb(3, 41, 119);
    internal static Color SoftBlue => IsDark ? Color.FromArgb(18, 63, 112) : Color.FromArgb(225, 244, 255);
    internal static Color TableHeader => IsDark ? Color.FromArgb(17, 55, 105) : Color.FromArgb(224, 241, 255);
    internal static Color AlternateRow => IsDark ? Color.FromArgb(15, 39, 82) : Color.FromArgb(245, 251, 255);
    internal static Color Selection => IsDark ? Color.FromArgb(18, 83, 142) : Color.FromArgb(204, 234, 255);
    internal static Color Cyan => Color.FromArgb(27, 177, 249);
    internal static Color Violet => Color.FromArgb(111, 72, 238);
    internal static Color Green => Color.FromArgb(42, 181, 84);
    internal static Color Orange => Color.FromArgb(245, 144, 20);
    internal static Color Pink => Color.FromArgb(238, 47, 117);

    internal static void Toggle()
    {
        IsDark = !IsDark;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
            File.WriteAllText(SettingsPath, IsDark ? "dark" : "light");
        }
        catch
        {
            // The selected theme still applies for the current session.
        }
    }
}
