using FTPClient.Models;

namespace FTPClient.Service.Interfaces
{
    public interface IFilesAndDirectoriesService
    {
        void SaveUserConfigFile(string profileName, string localPath);
        Profile GetUserSettings(string profileName);
        List<Profile> GetUserSettings();
        void SaveCurrentProfile(string profileName);
        void SaveCurrentProfile();
        string GetCurrentProfile();
    }
}
