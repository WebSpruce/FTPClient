using Avalonia.Controls;
using FTPClient.ViewModels;

namespace FTPClient.Views;

public partial class SettingsPageView : UserControl
{
    public static SettingsPageView instance;
    public SettingsPageView()
    {
        InitializeComponent();
        instance = this;
        DataContext = new SettingsPageViewModel();
    }
}