using Avalonia.Controls;
using Avalonia.Input;
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
    private async void PasteKeyDownCommand(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.V && e.KeyModifiers == KeyModifiers.Control)
        {
            var textBox = e.Source as TextBox;
            if (textBox != null)
            {
                var clipboard = new Window().Clipboard;
                textBox.Text = await clipboard.GetTextAsync();
            }
        }
    }
}