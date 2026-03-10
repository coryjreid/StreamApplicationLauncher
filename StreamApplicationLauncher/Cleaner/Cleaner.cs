using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using StreamApplicationLauncher.Data;
using StreamApplicationLauncher.Models;

namespace StreamApplicationLauncher.Cleaner;

public class Cleaner(LogManager logManager, PidSqliteDataService pidSqlite) {
    [DllImport("user32.dll")]
    private static extern bool PostThreadMessage(uint idThread, uint msg, IntPtr wParam, IntPtr lParam);

    private const uint WmQuit = 0x0012;

    public void Run() {
        Thread.Sleep(500);

        List<(int Pid, string Name, bool ForceKill)> pids = pidSqlite.GetAllPids();

        if (pids.Count == 0) {
            logManager.Info("No tracked processes found");
            return;
        }

        logManager.Info($"Found {pids.Count} tracked process{(pids.Count == 1 ? "" : "es")} to clean up");

        List<(Process Process, string Name)> gracefulCandidates = [];

        // Phase 1: send a graceful shutdown signal to every process, then collect them all
        // for a shared wait. Sending all signals before waiting means the 30s deadline is
        // shared across the entire set — a slow OBS shutdown doesn't starve other processes.
        // Processes marked forceKill are terminated immediately and skip the graceful path.
        foreach ((int storedPid, string storedName, bool forceKill) in pids) {
            Process liveProcess;
            try {
                liveProcess = Process.GetProcessById(storedPid);
            } catch (ArgumentException) {
                logManager.Info($"\"{storedName}\" (PID={storedPid}) is not running; skipping");
                continue;
            }

            // Recycling guard: PID exists but now belongs to a different process.
            // ProcessName omits the .exe extension, so strip it from the stored name before comparing.
            string storedBaseName = Path.GetFileNameWithoutExtension(storedName);
            if (!string.Equals(liveProcess.ProcessName, storedBaseName, StringComparison.OrdinalIgnoreCase)) {
                logManager.Warning($"\"{storedName}\" (PID={storedPid}) is now \"{liveProcess.ProcessName}\" — possible PID recycling; skipping");
                continue;
            }

            if (forceKill) {
                logManager.Info($"Force killing \"{storedName}\" (PID={storedPid})");
                liveProcess.Kill(entireProcessTree: true);
                logManager.Info($"\"{storedName}\" force killed");
                continue;
            }

            logManager.Info($"Requesting graceful close of \"{storedName}\" (PID={storedPid})");

            if (liveProcess.MainWindowHandle != IntPtr.Zero) {
                // Windowed process: WM_CLOSE to the main window triggers the normal close path.
                liveProcess.CloseMainWindow();
            } else {
                // No visible main window (e.g. OBS running virtual cam only, background engines).
                // Post WM_QUIT to every thread — threads running a message loop (Qt, Win32) will
                // process it and run their normal shutdown sequence; threads without one ignore it.
                foreach (ProcessThread thread in liveProcess.Threads) {
                    PostThreadMessage((uint)thread.Id, WmQuit, IntPtr.Zero, IntPtr.Zero);
                }
            }

            gracefulCandidates.Add((liveProcess, storedName));
        }

        if (gracefulCandidates.Count == 0) {
            pidSqlite.ClearAllPids();
            logManager.Info("PID database cleared");
            return;
        }

        // Phase 2: wait up to the graceful timeout, shared across all candidates.
        logManager.Info($"Waiting up to {Constants.CleanupGracefulWaitSeconds}s for graceful exit...");
        Stopwatch elapsed = Stopwatch.StartNew();

        foreach ((Process process, _) in gracefulCandidates) {
            int remaining = (Constants.CleanupGracefulWaitSeconds * 1000) - (int)elapsed.ElapsedMilliseconds;
            if (remaining > 0) {
                process.WaitForExit(remaining);
            }
        }

        // Phase 3: report the outcome and force kill any that didn't exit in time.
        foreach ((Process process, string name) in gracefulCandidates) {
            if (process.HasExited) {
                logManager.Info($"\"{name}\" exited gracefully");
            } else {
                logManager.Warning($"\"{name}\" did not exit within {Constants.CleanupGracefulWaitSeconds}s; forcing termination");
                process.Kill(entireProcessTree: true);
            }
        }

        pidSqlite.ClearAllPids();
        logManager.Info("PID database cleared");
    }
}
