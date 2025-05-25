using System.IO;
using System.Text.Json;
using System.Windows;
using AutoIt;
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
        foreach ((string applicationLaunchName, Process process, Window window) in _applicationLaunchList) {
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
                pid = AutoItX.ProcessWait(process.Name, process.Existence.WaitTimeoutSeconds);
            } else {
                pid = AutoItX.Run(process.Executable, process.WorkingDirectory);
            }

            int autoItXErrorCode = AutoItX.ErrorCode();
            if (pid == 0 || autoItXErrorCode != 0) {
                _logManager.Critical($"Launch \"{applicationLaunchName}\" failed "
                                     + $"(AutoItX Error = {autoItXErrorCode}); Aborting");
                return;
            }

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

            _logManager.Info($"Launch \"{applicationLaunchName}\" successful");
        }
    }

    private static int RunProcess(Process process) {
        return AutoItX.Run(
            process.Executable,
            string.IsNullOrWhiteSpace(process.WorkingDirectory) ? null : process.WorkingDirectory);
    }
}