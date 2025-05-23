using StreamApplicationLauncher.Models;
using StreamApplicationLauncher.Tests.Fakes;
using StreamApplicationLauncher.ViewModels;
using Xunit;

namespace StreamApplicationLauncher.Tests.ViewModels;

public class MainViewModelTests {
    [Fact]
    public void ViewModel_Initializes_WithLogMessagesAndScrollState() {
        LogManager logManager = new();
        FakeRuntimeTimer timer = new();
        MainViewModel vm = new(logManager, timer);

        Assert.NotNull(vm.LogMessages);
        Assert.NotNull(vm.ScrollState);
        Assert.NotNull(vm.ScrollToBottomCommand);
    }

    [Fact]
    public void ScrollToBottomCommand_RequestsScroll() {
        LogManager logManager = new();
        FakeRuntimeTimer timer = new();
        MainViewModel vm = new(logManager, timer);

        Assert.False(vm.ScrollState.IsScrollRequested);

        vm.ScrollToBottomCommand.Execute(null);

        Assert.True(vm.ScrollState.IsScrollRequested);
    }

    [Fact]
    public void DurationText_Updates_ReflectsRuntimeTimer() {
        LogManager logManager = new();
        FakeRuntimeTimer timer = new();
        MainViewModel vm = new(logManager, timer);

        string initial = vm.DurationText;

        timer.Tick(); // simulate 1 second
        string updated = vm.DurationText;

        Assert.NotEqual(initial, updated);
        Assert.Equal("00:00:01", updated);
    }
}