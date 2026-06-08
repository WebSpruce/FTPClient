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
}