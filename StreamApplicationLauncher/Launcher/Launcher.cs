using System.IO;
using System.Text.Json;
using AutoIt;
using StreamApplicationLauncher.Data;
using StreamApplicationLauncher.Models;
using StreamApplicationLauncher.Models.Json;

namespace StreamApplicationLauncher.Launcher;

public class Launcher {
    private readonly LogManager _logManager;
    private readonly PidSqliteDataService _pidSqlite;
    private readonly List<Program>? _programs;
    private const int WindowWaitTimeoutSeconds = 10;

    public Launcher(LogManager logManager, string configPath, PidSqliteDataService pidSqlite) {
        _logManager = logManager;
        _pidSqlite = pidSqlite;

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

        int programsCount = _programs.Count;
        int currentProgram = 0;
        foreach ((string programName, Process process, Window window) in _programs) {
            // Perform some validation
            if (window is { CloseOnOpen: true, HideOnOpen: true }) {
                _logManager.Critical("CloseOnOpen and HideOnOpen are mutually exclusive; Aborting");
                return;
            }

            bool skipWindowHandle = false;
            if (!string.IsNullOrWhiteSpace(window.Title) && window is { CloseOnOpen: false, HideOnOpen: false }) {
                _logManager.Warning(
                    $"{programName} has a window title defined but does not close or hide on open; Obtaining a window handle will be skipped");
                skipWindowHandle = true;
            }

            // Perform launch procedure
            _logManager.Info($"({++currentProgram}/{programsCount}) Starting application {programName}");
            int pid = AutoItX.Run(process.ExecutablePath, process.WorkingDirectory);
            if (pid == 0 || AutoItX.ErrorCode() != 0) {
                _logManager.Critical($"Application {programName} failed to start; Skipping");
                continue;
            }

            if (!skipWindowHandle) {
                _logManager.Info($"Waiting for {programName}'s window to exist (up to {WindowWaitTimeoutSeconds} seconds)");
                int windowHandle = AutoItX.WinWait(window.Title, null, WindowWaitTimeoutSeconds);
                _logManager.Info("Successfully obtained window handle");

                if (window.HideOnOpen) {
                    if (AutoItX.WinSetState(windowHandle, AutoItX.SW_HIDE) != 1) {
                        _logManager.Error($"Failed to hide {programName}'s window; Continuing");
                    }
                } else if (window.CloseOnOpen) {
                    if (AutoItX.WinClose(window.Title) != 1) {
                        _logManager.Error($"Failed to close {programName}'s window; Continuing");
                    }
                }
            }
        }
    }
}