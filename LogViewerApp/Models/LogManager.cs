using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace LogViewerApp.Models;

public class LogManager {
    private readonly ConcurrentQueue<LogMessage> _logQueue = new();
    private readonly DispatcherTimer _timer;

    public ObservableCollection<LogMessage> LogMessages { get; } = [];
    public Action? ScrollToBottomRequest { get; set; }

    public LogManager() {
        _timer = new DispatcherTimer {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _timer.Tick += OnTimerTick;
        _timer.Start();
    }

    public void EnqueueLog(LogLevel level, string message) {
        _logQueue.Enqueue(new LogMessage {
            Timestamp = DateTime.Now,
            Level = level,
            Message = message
        });
    }

    private void OnTimerTick(object? sender, EventArgs e) {
        while (_logQueue.TryDequeue(out LogMessage? logMessage)) {
            LogMessages.Add(logMessage);
        }
    }

    public void StartSimulatedLogging() {
        Task.Run(async () => {
            Random random = new();
            LogLevel[] levels = (LogLevel[])Enum.GetValues(typeof(LogLevel));

            while (true) {
                LogLevel level = levels[random.Next(levels.Length)];
                EnqueueLog(level, $"Simulated {level} log from thread {Environment.CurrentManagedThreadId}");
                await Task.Delay(500);
            }
        });
    }

    public void RequestScrollToBottom() {
        ScrollToBottomRequest?.Invoke();
    }
}