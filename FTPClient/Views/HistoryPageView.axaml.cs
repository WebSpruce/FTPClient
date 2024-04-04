using Avalonia.Controls;
using FTPClient.Models.Models;
using FTPClient.ViewModels;

namespace FTPClient.Views;

public partial class HistoryPageView : UserControl
{
    public HistoryPageView()
    {
        InitializeComponent();
        DataContext = new HistoryPageViewModel();
    }

    private void ConnectBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var item = (sender as Button).DataContext as Connection;
        HistoryPageViewModel.instance.Connect(item);
    }

    private void RemoveBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var item = (sender as Button).DataContext as Connection;
        HistoryPageViewModel.instance.Remove(item);
    }
}