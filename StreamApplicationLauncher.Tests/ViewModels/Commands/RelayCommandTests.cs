using StreamApplicationLauncher.ViewModels.Commands;
using Xunit;

namespace StreamApplicationLauncher.Tests.ViewModels.Commands;

public class RelayCommandTests {
    [Fact]
    public void CanExecute_ReturnsTrue_WhenNoPredicateProvided() {
        RelayCommand command = new(_ => { });

        bool canExecute = command.CanExecute(null);

        Assert.True(canExecute);
    }

    [Fact]
    public void CanExecute_RespectsPredicate() {
        RelayCommand command = new(_ => { }, _ => false);

        bool canExecute = command.CanExecute(null);

        Assert.False(canExecute);
    }

    [Fact]
    public void Execute_CallsAction() {
        bool wasCalled = false;
        RelayCommand command = new(_ => wasCalled = true);

        command.Execute(null);

        Assert.True(wasCalled);
    }

    [Fact]
    public void RaiseCanExecuteChanged_FiresEvent() {
        RelayCommand command = new(_ => { }, _ => true);

        bool eventFired = false;
        command.CanExecuteChanged += (_, _) => eventFired = true;

        command.RaiseCanExecuteChanged();

        Assert.True(eventFired);
    }
}