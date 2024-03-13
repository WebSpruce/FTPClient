using Avalonia.Controls;
using FTPClient.ViewModels;

namespace FTPClient.Views;

public partial class HomePageView : UserControl
{
    public static HomePageView instance;
    public HomePageView()
    {
        InitializeComponent();
        instance = this;
        DataContext = new HomePageViewModel();
    }
}