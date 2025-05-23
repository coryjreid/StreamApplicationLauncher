using StreamApplicationLauncher.Tests.Fakes;
using Xunit;

namespace StreamApplicationLauncher.Tests.Models;

public class RuntimeTimerTests {
    [Fact]
    public void DurationText_Updates_AfterTick() {
        FakeRuntimeTimer timer = new();
        string before = timer.DurationText;

        timer.Tick();

        Assert.NotEqual(before, timer.DurationText);
        Assert.Equal("00:00:01", timer.DurationText);
    }
}