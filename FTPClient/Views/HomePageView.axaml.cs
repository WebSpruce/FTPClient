using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using FTPClient.Models;
using FTPClient.Models.Models;
using FTPClient.ViewModels;

namespace FTPClient.Views;

public partial class HomePageView : UserControl
{
    public HomePageView()
    {
        InitializeComponent();
        this.Loaded += OnLoaded;
        ServerTreeView.AddHandler(TreeViewItem.ExpandedEvent, TreeViewItem_Expanded);
    }
    
    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is HomePageViewModel viewModel)
        {
            await viewModel.OnLoad();
        }
    }
    private async void PasteKeyDownCommand(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.V || e.KeyModifiers != KeyModifiers.Control) return;

        var textBox = e.Source as TextBox;
        if (textBox is null) return;
        
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard is null) return;

        textBox.Text = await clipboard.TryGetTextAsync();
    }

    private async void Directory_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        var mouseButton = e.GetCurrentPoint(this).Properties.PointerUpdateKind;

        if (sender is not StackPanel panel) return;
        if (panel.DataContext is not Directory selectedDirectory) return;
        if (DataContext is not HomePageViewModel viewModel) return;

        if (mouseButton == PointerUpdateKind.RightButtonPressed)
        {
            viewModel.SelectedServerItem = selectedDirectory;

            var contextMenu = BuildDirectoryContextMenu(viewModel, selectedDirectory);
            contextMenu.Open(panel);
        }
        else if (mouseButton == PointerUpdateKind.LeftButtonPressed)
        {
            viewModel.SelectedServerItem = selectedDirectory;
        }
    }

    private void File_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        var mouseButton = e.GetCurrentPoint(this).Properties.PointerUpdateKind;

        if (sender is not StackPanel panel) return;
        if (panel.DataContext is not FileItem selectedFile) return;
        if (DataContext is not HomePageViewModel viewModel) return; 

        if (mouseButton == PointerUpdateKind.RightButtonPressed)
        {
            viewModel.SelectedFileItem = selectedFile;

            var contextMenu = BuildFileContextMenu(viewModel, selectedFile);
            contextMenu.Open(panel);
        }
    }
    
    private static ContextMenu BuildDirectoryContextMenu(HomePageViewModel viewModel, Directory directory)
    {
        var createDirectory = new MenuItem { Header = "Create Directory" };
        var createFile = new MenuItem { Header = "Create File" };
        var rename = new MenuItem { Header = "Rename" };
        var delete = new MenuItem { Header = "Delete" };

        createDirectory.Click += (_, _) => viewModel.OpenCreateDirectoryForm();
        createFile.Click += (_, _) => viewModel.OpenCreateFileForm();
        rename.Click += (_, _) => viewModel.OpenRenameForm(directory);
        delete.Click += (_, _) => viewModel.DeleteFileOrDirectoryFromServerCommand.Execute(directory);

        var contextMenu = new ContextMenu();
        contextMenu.Items.Add(createDirectory);
        contextMenu.Items.Add(createFile);
        contextMenu.Items.Add(rename);
        contextMenu.Items.Add(delete);

        return contextMenu;
    }

    private static ContextMenu BuildFileContextMenu(HomePageViewModel viewModel, FileItem file)
    {
        var rename = new MenuItem { Header = "Rename" };
        var delete = new MenuItem { Header = "Delete" };

        rename.Click += (_, _) => viewModel.OpenRenameForm(file);
        delete.Click += (_, _) => viewModel.DeleteFileOrDirectoryFromServerCommand.Execute(file);

        var contextMenu = new ContextMenu();
        contextMenu.Items.Add(rename);
        contextMenu.Items.Add(delete);

        return contextMenu;
    }
    
    private async void TreeViewItem_Expanded(object? sender, RoutedEventArgs e)
    {
        if (e.Source is not TreeViewItem treeViewItem) return;
        if (treeViewItem.DataContext is not Directory directory) return;
        if (DataContext is not HomePageViewModel viewModel) return;

        await viewModel.LoadDirectoryChildrenOnDemandCommand.ExecuteAsync(directory);
    }
}