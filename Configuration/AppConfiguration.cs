using Microsoft.Extensions.Configuration;

namespace PropertyManagementSystem.Configuration;

public static class AppConfiguration
{
    private const string SettingsFileName = "appsettings.json";
    private const string ConnectionName = "SyrentoDatabase";

    public static IConfigurationRoot Build()
    {
        var settingsBasePath = File.Exists(Path.Combine(
            AppContext.BaseDirectory,
            SettingsFileName))
            ? AppContext.BaseDirectory
            : Directory.GetCurrentDirectory();

        return new ConfigurationBuilder()
            .SetBasePath(settingsBasePath)
            .AddJsonFile(SettingsFileName, optional: false, reloadOnChange: false)
            .Build();
    }

    public static string GetConnectionString()
    {
        return Build().GetConnectionString(ConnectionName)
            ?? throw new InvalidOperationException(
                $"Connection string '{ConnectionName}' was not found.");
    }
}
