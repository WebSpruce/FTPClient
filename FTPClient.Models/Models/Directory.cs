namespace FTPClient.Models;

public class Directory : FileItem
{
    public List<FileItem> FileItems { get; set; }

    public Directory()
    {
        FileItems = new();
    }
}

public class FileItem
{
    public string Name { get; set; }
    public string Path { get; set; }
    public string Size { get; set; }
}