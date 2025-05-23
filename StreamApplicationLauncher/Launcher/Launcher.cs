using System.IO;
using System.Text.Json;
using StreamApplicationLauncher.Models;
using StreamApplicationLauncher.Models.Json;

namespace StreamApplicationLauncher.Launcher;

public class Launcher {
    private readonly LogManager _logManager;
    private readonly List<Program>? _programs;

    public Launcher(LogManager logManager, string configPath) {
        _logManager = logManager;

        if (!File.Exists(configPath)) {
            _logManager.Log(LogLevel.Critical, $"Config file not found: {configPath}");
            return;
        }

        try {
            string json = File.ReadAllText(configPath);
            _programs = JsonSerializer.Deserialize<List<Program>>(json, Constants.DefaultJsonSerializerOptions);
        } catch (Exception ex) {
            _logManager.Log(LogLevel.Critical, $"Failed to load programs file: {ex.Message}");
        }
    }

    public void Run() {
        Thread.Sleep(2000);
        _logManager.Log(LogLevel.Info,
            $"Loaded {_programs!.Count} {(_programs!.Count == 1 ? "program" : "programs")} for launch");
        _logManager.Log(LogLevel.Info, "The Run() method was called");
    }
}