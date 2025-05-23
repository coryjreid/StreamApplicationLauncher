using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.Xaml.Behaviors;

namespace LogViewerApp.Behaviors;

public class AutoScrollBehavior : Behavior<ListBox> {
    private bool _userScrolled = false;

    protected override void OnAttached() {
        base.OnAttached();

        if (AssociatedObject.Items is INotifyCollectionChanged notifyCollection) {
            notifyCollection.CollectionChanged += OnCollectionChanged;
        }

        AssociatedObject.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(OnScrollChanged));
    }

    protected override void OnDetaching() {
        base.OnDetaching();

        if (AssociatedObject.Items is INotifyCollectionChanged notifyCollection) {
            notifyCollection.CollectionChanged -= OnCollectionChanged;
        }

        AssociatedObject.RemoveHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(OnScrollChanged));
    }

    private void OnScrollChanged(object sender, ScrollChangedEventArgs e) {
        if (e.OriginalSource is not ScrollViewer scrollViewer) {
            return;
        }

        bool atBottom = scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight - 1;
        _userScrolled = !atBottom;
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        if (!_userScrolled && AssociatedObject.ItemContainerGenerator.Status ==
            GeneratorStatus.ContainersGenerated) {
            AssociatedObject.ScrollIntoView(AssociatedObject.Items[^1]);
        }
    }
}