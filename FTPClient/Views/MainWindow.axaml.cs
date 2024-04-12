using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using FTPClient.ViewModels;

namespace FTPClient.Views;

public partial class MainWindow : Window
{
    private bool _mouseDownForWindowMoving = false;
    private PointerPoint _originalPoint;
    public bool isWindowHover = false;
    public static MainWindow instance;
    public MainWindow()
    {
        InitializeComponent();
        instance = this;
        DataContext = new MainWindowViewModel();
    }

    private void InputElement_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_mouseDownForWindowMoving) return;

        PointerPoint currentPoint = e.GetCurrentPoint(this);
        Position = new PixelPoint(Position.X + (int)(currentPoint.Position.X - _originalPoint.Position.X),
            Position.Y + (int)(currentPoint.Position.Y - _originalPoint.Position.Y));
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (WindowState == WindowState.Maximized || WindowState == WindowState.FullScreen) return;

        if (isWindowHover)
        {
            _mouseDownForWindowMoving = false;
            return;
        }
        else
        {
            _mouseDownForWindowMoving = true;
            _originalPoint = e.GetCurrentPoint(this);
        }

       
    }

    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _mouseDownForWindowMoving = false;
    }
}