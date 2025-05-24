using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace StreamApplicationLauncher.Models;

public class LogManager {
    private readonly ConcurrentQueue<LogMessage> _logQueue = new();
    private readonly DispatcherTimer _timer;

    public ObservableCollection<LogMessage> LogMessages { get; } = [];

    public LogManager() {
        _timer = new DispatcherTimer {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _timer.Tick += (_, _) => FlushQueue();
        _timer.Start();
    }

    public void Log(LogLevel level, string message) {
        _logQueue.Enqueue(new LogMessage(level, message));
    }

    public void Trace(string message) {
        _logQueue.Enqueue(new LogMessage(LogLevel.Trace, message));
    }

    public void Info(string message) {
        _logQueue.Enqueue(new LogMessage(LogLevel.Info, message));
    }

    public void Debug(string message) {
        _logQueue.Enqueue(new LogMessage(LogLevel.Debug, message));
    }

    public void Warning(string message) {
        _logQueue.Enqueue(new LogMessage(LogLevel.Warning, message));
    }

    public void Error(string message) {
        _logQueue.Enqueue(new LogMessage(LogLevel.Error, message));
    }

    public void Critical(string message) {
        _logQueue.Enqueue(new LogMessage(LogLevel.Critical, message));
    }

    // ✅ Testable flush method
    public void FlushQueue() {
        while (_logQueue.TryDequeue(out LogMessage? log)) {
            LogMessages.Add(log);
        }
    }

    protected void StartSimulatedLogging() {
        Task.Run(async () => {
            Random random = new();
            LogLevel[] levels = (LogLevel[])Enum.GetValues(typeof(LogLevel));

            while (true) {
                LogLevel level = levels[random.Next(levels.Length)];
                Log(level, $"Simulated {level} log");
                await Task.Delay(500);
            }
        });
    }
}