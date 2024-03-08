using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FTPClient.ViewModels;

namespace FTPClient.Views;

public partial class HomePageView : UserControl
{
    public HomePageView()
    {
        InitializeComponent();
        DataContext = new HomePageViewModel();
    }
}