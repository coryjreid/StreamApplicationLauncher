using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StreamApplicationLauncher.Models;

public static class Constants {
    public static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new() {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

    public static readonly string ApplicationStoragePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Aezshma",
        "StreamApplicationLauncher");

    public const string PidDatabaseFileName = "pids.db";

    public const int ApplicationAutoShutdownDelaySeconds = 10;

    // OBS needs time to stop outputs, finalize recordings, and tear down browser sources.
    // 30 seconds covers worst-case graceful shutdown before force-killing stragglers.
    public const int CleanupGracefulWaitSeconds = 30;
}