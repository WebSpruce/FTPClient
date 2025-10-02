using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Input;
using FTPClient.Models;
using FTPClient.Models.Models;
using FTPClient.ViewModels;

namespace FTPClient.Views;

public partial class HomePageView : UserControl
{
    public static HomePageView instance;
    public Border newDirectoryForm;
    public Border newFileForm;
    public Border renameForm;
    public HomePageView()
    {
        InitializeComponent();
        newDirectoryForm = NewDirectoryForm;
        newFileForm = NewFileForm;
        renameForm = RenameForm;
        instance = this;
        DataContext = new HomePageViewModel();
    }
    public HomePageView(Connection connection)
    {
        InitializeComponent();
        instance = this;
        DataContext = new HomePageViewModel(connection);
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

    private async void Directory_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        var mouseButton = e.GetCurrentPoint(this).Properties.PointerUpdateKind;
        var selectedDirectory = (sender as StackPanel)?.DataContext as Directory;
    
        if (selectedDirectory == null) return;
        
        var viewModel = this.DataContext as HomePageViewModel;
        if (viewModel == null) return;
        Debug.WriteLine($"mouse clicked: {mouseButton}");
        if (mouseButton == PointerUpdateKind.RightButtonPressed)
        {
            var contextMenu = new ContextMenu();
            var createDirectory = new MenuItem { Header = "Create Directory" };
            var createFile = new MenuItem { Header = "Create File" };
            var Rename = new MenuItem { Header = "Rename" };
            var Delete = new MenuItem { Header = "Delete" };

            contextMenu.Items.Add(createDirectory);
            contextMenu.Items.Add(createFile);
            contextMenu.Items.Add(Rename);
            contextMenu.Open((Control)sender);
        
            createDirectory.Click += (sender, e) =>
            {
                HomePageViewModel.instance.OpenCreateDirectoryForm();
            };
            createFile.Click += (sender, e) =>
            {
                HomePageViewModel.instance.OpenCreateFileForm();
            };
            Rename.Click += (sender, e) =>
            {
                HomePageViewModel.instance.OpenRenameForm(selectedDirectory);
            };
        }
        else if (mouseButton == PointerUpdateKind.LeftButtonPressed)
        {
            Debug.WriteLine($"mouse: left clicked");
            viewModel.SelectedServerItem = selectedDirectory;
            await viewModel.LoadDirectoryChildrenOnDemand(selectedDirectory);
        }
    }

    private void File_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        var mouseButton = e.GetCurrentPoint(this).Properties.PointerUpdateKind;
        var selectedFile = (sender as StackPanel).DataContext as FileItem;
        if (mouseButton == PointerUpdateKind.RightButtonPressed)
        {
            var contextMenu = new ContextMenu();
            var Rename = new MenuItem { Header = "Rename" };

            contextMenu.Items.Add(Rename);
            contextMenu.Open((Control)sender);
            Rename.Click += (sender, e) =>
            {
                HomePageViewModel.instance.OpenRenameForm(selectedFile);
            };
        }
    }
}