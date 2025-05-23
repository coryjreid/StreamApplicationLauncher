using System.IO;
using System.Text.Json;
using StreamApplicationLauncher.Models;
using StreamApplicationLauncher.Models.Json;

namespace StreamApplicationLauncher.Launcher;

public class Launcher {
    private readonly LogManager _logManager;

    public Launcher(LogManager logManager, string configPath) {
        _logManager = logManager;

        if (!File.Exists(configPath)) {
            _logManager.Log(LogLevel.Critical, $"Config file not found: {configPath}");
            return;
        }

        try {
            string json = File.ReadAllText(configPath);
            // Replace with real deserialization logic as needed
            List<Program>? programs = JsonSerializer.Deserialize<List<Program>>(json, Constants.DefaultJsonSerializerOptions);
            _logManager.Log(LogLevel.Info, $"Loaded config: {json.Length} characters");
        } catch (Exception ex) {
            _logManager.Log(LogLevel.Critical, $"Failed to load config: {ex.Message}");
        }
    }

    public void Run() {
        _logManager.Log(LogLevel.Info, "The Run() method was called.");
    }
}