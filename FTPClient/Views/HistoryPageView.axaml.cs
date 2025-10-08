using Avalonia.Controls;
using Avalonia.Interactivity;
using FTPClient.Database.Interfaces;
using FTPClient.Database.Repository;
using FTPClient.Models.Models;
using FTPClient.ViewModels;

namespace FTPClient.Views;

public partial class HistoryPageView : UserControl
{
    public HistoryPageView()
    {
        InitializeComponent();
        this.Loaded += OnLoaded;
    }
    
    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is HistoryPageViewModel viewModel)
        {
            await viewModel.OnLoad();
        }
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