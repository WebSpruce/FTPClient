using Avalonia.Controls;
using FTPClient.ViewModels;
using System.Diagnostics;

namespace FTPClient.Views;

public partial class SettingsPageView : UserControl
{
    public static SettingsPageView instance;
    public bool isWindowHover = false;
    public SettingsPageView()
    {
        InitializeComponent();
        instance = this;
    }

    private void ComboBox_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        MainWindow.instance.isWindowHover = true;
    }

    private void ComboBox_PointerExited(object? sender, Avalonia.Input.PointerEventArgs e)
    {
        MainWindow.instance.isWindowHover = false;
    }

    private void ColorPicker_ColorChanged(object? sender, Avalonia.Controls.ColorChangedEventArgs e)
    {
        SettingsPageViewModel.instance.ColorPickerColorChanged(e.NewColor);
    }
}