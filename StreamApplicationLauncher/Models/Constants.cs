using System.IO;
using System.Text.Json;

namespace StreamApplicationLauncher.Models;

public static class Constants {
    public static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new() {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public static readonly string ApplicationStoragePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Aezshma",
        "StreamApplicationLauncher");

    public static readonly string PidDatabaseFileName = "pids.db";
}