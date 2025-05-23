using System.Windows;
using LogViewerApp.Models;
using LogViewerApp.ViewModels;

namespace LogViewerApp.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        LogManager logManager = new();
        logManager.StartSimulatedLogging();

        MainViewModel viewModel = new(logManager);
        DataContext = viewModel;
    }
}