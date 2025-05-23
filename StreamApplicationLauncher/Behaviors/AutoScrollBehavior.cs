using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;
using StreamApplicationLauncher.ViewModels;

namespace StreamApplicationLauncher.Behaviors;

public class AutoScrollBehavior : Behavior<ListBox> {
    private bool _userScrolled = false;

    public static readonly DependencyProperty ScrollStateProperty =
        DependencyProperty.Register(nameof(ScrollState), typeof(ScrollState), typeof(AutoScrollBehavior),
            new PropertyMetadata(null, OnScrollStateChanged));

    public ScrollState? ScrollState {
        get => (ScrollState?)GetValue(ScrollStateProperty);
        set => SetValue(ScrollStateProperty, value);
    }

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

    private static void OnScrollStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (d is AutoScrollBehavior behavior) {
            if (e.OldValue is ScrollState oldState) {
                oldState.PropertyChanged -= behavior.OnScrollStatePropertyChanged;
            }

            if (e.NewValue is ScrollState newState) {
                newState.PropertyChanged += behavior.OnScrollStatePropertyChanged;
            }
        }
    }

    private void OnScrollStatePropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(ScrollState.IsScrollRequested) && ScrollState?.IsScrollRequested == true) {
            _userScrolled = false;
            if (AssociatedObject.Items.Count > 0) {
                AssociatedObject.ScrollIntoView(AssociatedObject.Items[^1]);
            }

            // Reset request
            ScrollState.IsScrollRequested = false;
        }
    }

    private void OnScrollChanged(object sender, ScrollChangedEventArgs e) {
        if (e.OriginalSource is not ScrollViewer scrollViewer) {
            return;
        }

        bool atBottom = scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight - 1;
        _userScrolled = !atBottom;

        if (ScrollState is not null) {
            ScrollState.IsAtBottom = atBottom;
        }
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        if (!_userScrolled && AssociatedObject.Items.Count > 0) {
            AssociatedObject.ScrollIntoView(AssociatedObject.Items[^1]);
        }
    }
}