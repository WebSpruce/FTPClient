using FTPClient.Models;
using Renci.SshNet;

namespace FTPClient.Service.Interfaces
{
    public interface IFilesAndDirectoriesService
    {
        void SaveUserConfigFile(Profile profile);
        Profile GetUserSettings(string profileName);
        List<Profile> GetUserSettings();
        void SaveCurrentProfile(string profileName);
        void SaveCurrentProfile();
        string GetCurrentProfile();
        void AddNewProfile(string newProfileName);
        void DeleteProfile(string profileName);
    }
}
