using LogViewerApp.Models;
using Xunit;

namespace LogViewerApp.Tests.Models;

public class LogManagerTests {
    [Fact]
    public void EnqueueLog_ThenFlushQueue_AddsToLogMessages() {
        LogManager manager = new LogManager();
        manager.EnqueueLog(LogLevel.Warning, "Test log");

        manager.FlushQueue();

        Assert.Single(manager.LogMessages);
        Assert.Equal("Test log", manager.LogMessages[0].Message);
    }

    [Fact]
    public void FlushQueue_WithNoPendingLogs_DoesNotModifyLogMessages() {
        LogManager manager = new LogManager();
        manager.FlushQueue();

        Assert.Empty(manager.LogMessages);
    }
}