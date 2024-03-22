namespace FTPClient.Service.Interfaces
{
    public interface IFilesAndDirectoriesService
    {
        void SaveUserConfigFile(string localPath);
        string GetUserSettings();
    }
}
