namespace FTPClient.Service.Helper;

public static class DataFormating
{
    public static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
    
        while (len >= 1000 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1000;
        }
    
        return $"{len:0.##} {sizes[order]}";
    }
}