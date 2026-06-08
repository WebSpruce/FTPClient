using Avalonia.Controls;
using FTPClient.ViewModels;
using Avalonia.Interactivity;

namespace FTPClient.Views;

public partial class SettingsPageView : UserControl
{
    public SettingsPageView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsPageViewModel vm)
            await vm.OnLoad();
    }

    private void ComboBox_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        var mainWindow = TopLevel.GetTopLevel(this) as MainWindow;
        if (mainWindow is null) return;
        mainWindow.IsWindowHover = true;
    }

    private void ComboBox_PointerExited(object? sender, Avalonia.Input.PointerEventArgs e)
    {
        var mainWindow = TopLevel.GetTopLevel(this) as MainWindow;
        if (mainWindow is null) return;
        mainWindow.IsWindowHover = false;
    }

    private void ColorPicker_ColorChanged(object? sender, Avalonia.Controls.ColorChangedEventArgs e)
    {
        if (DataContext is not SettingsPageViewModel vm) return;
        vm.ColorPickerColorChanged(e.NewColor);
    }
}