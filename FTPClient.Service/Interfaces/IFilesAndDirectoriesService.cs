using FTPClient.Models;
using FTPClient.Models.Models;

namespace FTPClient.Service.Interfaces
{
    public interface IFilesAndDirectoriesService
    {
        Result SaveUserConfigFile(Profile profile);
        Result<Profile> GetUserSettings(string profileName);
        Result<List<Profile>> GetUserSettings();
        Result SaveCurrentProfile(string profileName);
        Result SaveCurrentProfile();
        Result<string> GetCurrentProfile();
        Result AddNewProfile(string newProfileName);
        Result DeleteProfile(string profileName);
    }
}
