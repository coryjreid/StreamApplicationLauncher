using System.IO;
using System.Text.Json;
using AutoIt;
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
        _logManager.Info($"Loaded {_programs!.Count} {(_programs!.Count == 1 ? "program" : "programs")} for launch");
        _logManager.Info("The Run() method was called");

        int programsCount = _programs.Count;
        int currentProgram = 0;
        foreach (Program program in _programs) {
            _logManager.Info($"({++currentProgram}/{programsCount}) Starting application {program.Name}");
            int result = AutoItX.Run(program.Process.ExecutablePath, program.Process.WorkingDirectory);
            int errorCode = AutoItX.ErrorCode();
            if (result == 1 || errorCode == 1) {
                _logManager.Critical($"Application {program.Name} failed to start");
            }
        }
    }
}