namespace StreamApplicationLauncher.Models;

public enum LogLevel {
    Trace,
    Debug,
    Info,
    Warning,
    Error,
    Critical
}

public record LogMessage(LogLevel Level, string Message) {
    public DateTime Timestamp { get; init; } = DateTime.Now;

    public override string ToString() {
        return $"[{Timestamp:MM/dd/yyyy hh:mm:ss tt}] [{Level}] {Message}";
    }
}