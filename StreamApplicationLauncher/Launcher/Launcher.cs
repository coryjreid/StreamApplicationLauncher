using System.IO;
using System.Text.Json;
using System.Windows;
using AutoIt;
using Microsoft.Win32;
using StreamApplicationLauncher.Data;
using StreamApplicationLauncher.Models;
using StreamApplicationLauncher.Models.Json;
using Action = StreamApplicationLauncher.Models.Json.Action;
using Window = StreamApplicationLauncher.Models.Json.Window;

namespace StreamApplicationLauncher.Launcher;

public class Launcher {
    private readonly LogManager _logManager;
    private readonly PidSqliteDataService _pidSqlite;
    private readonly List<ApplicationLaunch>? _applicationLaunchList;

    public Launcher(LogManager logManager, string configPath, PidSqliteDataService pidSqlite) {
        _logManager = logManager;
        _pidSqlite = pidSqlite;

        if (!File.Exists(configPath)) {
            _logManager.Log(LogLevel.Critical, $"Config file not found: {configPath}");
            return;
        }

        try {
            string json = File.ReadAllText(configPath);
            _applicationLaunchList
                = JsonSerializer.Deserialize<List<ApplicationLaunch>>(json, Constants.DefaultJsonSerializerOptions);

            if (_applicationLaunchList == null) {
                MessageBox.Show(
                    Application.Current.MainWindow!,
                    "Unable to deserialize launch list from provided file",
                    "Fatal Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Application.Current.Dispatcher.Invoke(() => Application.Current.Shutdown(1));
            }
        } catch (Exception ex) {
            MessageBox.Show(
                Application.Current.MainWindow!,
                $"Failed to load launch list: {ex.Message}",
                "Fatal Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Application.Current.Dispatcher.Invoke(() => Application.Current.Shutdown(1));
        }
    }

    public void Run() {
        // Wait for the window to open before beginning
        Thread.Sleep(500);

        // Configure some settings
        AutoItX.AutoItSetOption("WinTitleMatchMode", 4);


        _logManager.Info(
            $"Loaded {_applicationLaunchList!.Count} application launch{(_applicationLaunchList!.Count == 1 ? "" : "es")}");

        int programsCount = _applicationLaunchList.Count;
        int currentProgram = 0;
        foreach ((string applicationLaunchName, Process process, Window window, List<AutoItScript> postLaunchScripts) in
                 _applicationLaunchList) {
            bool skipWindowHandle = window.OnOpen.Action == Action.None;
            if (!skipWindowHandle && string.IsNullOrWhiteSpace(window.Title)) {
                _logManager.Critical($"Launch \"{applicationLaunchName}\" defines "
                                     + $"ActionOnOpen={window.OnOpen.Action} but Title=\"{window.Title}\"; Aborting");
                return;
            }

            // Perform launch procedure
            _logManager.Info($"Launch \"{applicationLaunchName}\" ({++currentProgram}/{programsCount}) starting...");

            int pid;
            if (process.Existence.WaitExists) {
                _ = RunProcess(process);

                int waitResult = AutoItX.ProcessWait(process.Name, process.Existence.WaitTimeoutSeconds);
                if (waitResult == 0) {
                    _logManager.Critical($"Failed locating \"{process.Name}\" process; Aborting");
                    return;
                }

                pid = AutoItX.ProcessExists(process.Name);
            } else {
                pid = RunProcess(process);
            }

            int autoItXErrorCode = AutoItX.ErrorCode();
            if (pid == 0 || autoItXErrorCode != 0) {
                _logManager.Critical($"Launch \"{applicationLaunchName}\" failed "
                                     + $"(AutoItX Error = {autoItXErrorCode}); Aborting");
                return;
            }

            _logManager.Info($"\"{applicationLaunchName}\" started; PID={pid}");

            if (!skipWindowHandle) {
                string waitTimeString = window.OnOpen.ActionDelaySeconds switch {
                    0 => "indefinitely",
                    1 => "for 1 second",
                    _ => $"for {window.OnOpen.ActionDelaySeconds} seconds"
                };
                _logManager.Info($"Awaiting \"{applicationLaunchName}\" window existence {waitTimeString}");

                int winWaitResult = AutoItX.WinWait(window.Title, null, window.OnOpen.ActionDelaySeconds);
                if (winWaitResult != 1) {
                    _logManager.Warning($"Failed detecting \"{applicationLaunchName}\" window existence; Proceeding");
                }

                _logManager.Info($"Obtaining \"{applicationLaunchName}\" window handle");
                IntPtr windowHandle = AutoItX.WinGetHandle(window.Title);
                if (windowHandle == 0) {
                    _logManager.Error($"Failed obtaining \"{applicationLaunchName}\" window handle; Skipping window control");
                    continue;
                }

                _logManager.Info($"Obtained \"{applicationLaunchName}\" window handle");
                if (window.OnOpen.ActionDelaySeconds > 0 && window.OnOpen.Action != Action.None) {
                    _logManager.Info($"Control window for \"{applicationLaunchName}\" waiting {window.OnOpen.ActionDelaySeconds} "
                                     + $"{(window.OnOpen.ActionDelaySeconds == 1 ? "second" : "seconds")} for initialization");
                    Thread.Sleep(1000 * window.OnOpen.ActionDelaySeconds);
                }

                switch (window.OnOpen.Action) {
                    case Action.Close:
                        if (AutoItX.WinClose(window.Title) == 1) {
                            _logManager.Info($"Closed \"{applicationLaunchName}\" window");
                        } else {
                            _logManager.Warning($"Failed closing \"{applicationLaunchName}\" window");
                        }

                        break;
                    case Action.Hide:
                        if (AutoItX.WinSetState(windowHandle, AutoItX.SW_HIDE) == 1) {
                            _logManager.Info($"Hid \"{applicationLaunchName}\" window");
                        } else {
                            _logManager.Warning($"Failed hiding \"{applicationLaunchName}\" window");
                        }

                        break;
                    case Action.Minimize:
                        if (AutoItX.WinSetState(windowHandle, AutoItX.SW_MINIMIZE) == 1) {
                            _logManager.Info($"Minimized \"{applicationLaunchName}\" window");
                        } else {
                            _logManager.Warning($"Failed minimizing \"{applicationLaunchName}\" window");
                        }

                        break;
                    case Action.None:
                    default:
                        break;
                }
            }

            int numberOfScripts = postLaunchScripts.Count;
            if (numberOfScripts != 0) {
                _logManager.Info($"Launch \"{applicationLaunchName}\" has {numberOfScripts} post-launch "
                                 + $"script{(numberOfScripts == 1 ? "" : "s")} to execute");
                
                const int currentScriptNumber = 1;
                foreach ((string? scriptPath, bool isRunWait) in postLaunchScripts) {
                    try {
                        if (string.IsNullOrWhiteSpace(scriptPath)) {
                            _logManager.Error($"Cannot run \"{applicationLaunchName}\" script "
                                              + $"{currentScriptNumber}/{numberOfScripts} as no path is provided; Continuing");
                            continue;
                        }
                        
                        _logManager.Info($"Running \"{applicationLaunchName}\" post-launch script "
                                         + $"{currentScriptNumber}/{numberOfScripts}"
                                         + (isRunWait ? ", and waiting for its completion" : ""));
                        
                        string autoItExe = GetAutoItExePath2(Environment.Is64BitProcess);
                        string invocationString = $"\"{autoItExe}\" /ErrorStdOut \"{scriptPath}\"";
                        int runWaitExitCode = 0;
                        int scriptPid = 0;
                        if (isRunWait) {
                            runWaitExitCode = AutoItX.RunWait(invocationString, null);
                        } else {
                            scriptPid = AutoItX.Run(invocationString, null);
                        }

                        int scriptAutoItXErrorCode = AutoItX.ErrorCode();
                        if ((scriptPid == 0 && !isRunWait) || runWaitExitCode != 0 || scriptAutoItXErrorCode != 0) {
                            _logManager.Critical($"\"{applicationLaunchName}\" script "
                                                 + $"{currentScriptNumber}/{numberOfScripts} failed "
                                                 + $"(ScriptPid = {scriptPid}, RunWaitExitCode = {runWaitExitCode}, "
                                                 + $"AutoItX Error = {scriptAutoItXErrorCode}); Aborting");
                            return;
                        }
                        
                        _logManager.Info($"Successfully ran \"{applicationLaunchName}\" post-launch script "
                                         + $"{currentScriptNumber}/{numberOfScripts}");
                    } catch (Exception exception) when (exception is InvalidOperationException or FileNotFoundException) {
                        _logManager.Critical($"Failed locating AutoIt executable: {exception.Message}; Aborting");
                        return;
                    }
                }
            }

            _logManager.Info($"Launch \"{applicationLaunchName}\" successful");
        }
    }

    private static string GetAutoItExePath2(bool x64) {
        string? exePath = TryGetAutoItExeFromRegistry(x64 ? RegistryView.Registry64 : RegistryView.Registry32);

        if (exePath == null && x64) {
            exePath = TryGetAutoItExeFromRegistry(RegistryView.Registry32); // fallback to 32-bit install
        }

        exePath ??= TryFindPortableAutoIt();
        if (exePath == null) {
            throw new InvalidOperationException("Could not locate AutoIt3 executable in registry or known folders.");
        }

        return exePath;
    }

    private static string? TryGetAutoItExeFromRegistry(RegistryView view) {
        const string keyPath = @"SOFTWARE\AutoIt v3\AutoIt";
        const string valueName = "InstallDir";

        using RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view);
        using RegistryKey? autoItKey = baseKey.OpenSubKey(keyPath);
        if (autoItKey == null) {
            return null;
        }

        string? installDir = autoItKey.GetValue(valueName) as string;
        if (string.IsNullOrWhiteSpace(installDir)) {
            return null;
        }

        string exeName = view == RegistryView.Registry64 ? "AutoIt3_x64.exe" : "AutoIt3.exe";
        string exePath = Path.Combine(installDir, exeName);

        return File.Exists(exePath) ? exePath : null;
    }

    private static string? TryFindPortableAutoIt() {
        string baseDir = AppContext.BaseDirectory;

        string[] candidates = [
            Path.Combine(baseDir, "AutoIt3_x64.exe"),
            Path.Combine(baseDir, "AutoIt3.exe"),
            Path.Combine(baseDir, "tools", "autoit", "AutoIt3_x64.exe"),
            Path.Combine(baseDir, "tools", "autoit", "AutoIt3.exe")
        ];

        return candidates.FirstOrDefault(File.Exists);
    }

    private static int RunProcess(Process process) {
        return AutoItX.Run(
            process.Executable,
            string.IsNullOrWhiteSpace(process.WorkingDirectory) ? null : process.WorkingDirectory);
    }
}