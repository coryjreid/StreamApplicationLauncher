using LogViewerApp.ViewModels;
using Xunit;

namespace LogViewerApp.Tests.ViewModels;

public class ScrollStateTests {
    [Fact]
    public void IsAtBottom_Change_RaisesPropertyChanged() {
        ScrollState state = new();
        bool raised = false;

        state.PropertyChanged += (_, e) => {
            if (e.PropertyName == nameof(ScrollState.IsAtBottom)) raised = true;
        };

        state.IsAtBottom = false;

        Assert.True(raised);
    }

    [Fact]
    public void IsScrollRequested_Change_RaisesPropertyChanged() {
        ScrollState state = new();
        bool raised = false;

        state.PropertyChanged += (_, e) => {
            if (e.PropertyName == nameof(ScrollState.IsScrollRequested)) raised = true;
        };

        state.IsScrollRequested = true;

        Assert.True(raised);
    }
}