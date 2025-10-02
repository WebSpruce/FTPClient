using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace FTPClient.Models;

public class Directory : FileItem, INotifyPropertyChanged
{
    public ObservableCollection<FileItem> FileItems { get; set; }
    public bool HasChildren { get; set; } = true;
    public bool ChildrenLoaded { get; set; } = false;
    public bool IsLoading { get; set; } = false;
    public Directory()
    {
        FileItems = new();
    }
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

public class FileItem
{
    public string Name { get; set; }
    public string Path { get; set; }
    public string Size { get; set; }
}