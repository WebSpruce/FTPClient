using FTPClient.Models;
using FTPClient.Service.Interfaces;
using System.Diagnostics;
using System.Text.Json;

namespace FTPClient.Service.Services
{
    public class FilesAndDirectoriesService : IFilesAndDirectoriesService
    {
        private static string settingsFilePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/userSettings.json";
        public void SaveUserConfigFile(string localPath)
        {
            try
            {
                List<UserSettings> userSettings = new List<UserSettings>();
                userSettings.Add(new UserSettings
                {
                    LocalPath = localPath,
                });

                string jsonFile = JsonSerializer.Serialize(userSettings);
                File.WriteAllText(settingsFilePath, jsonFile);
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"FilesAndDirectoriesService SaveUserConfigFile error: {ex}");
            }
            
        }
        public string GetUserSettings()
        {
            try
            {
                var userSettings = File.ReadAllText(settingsFilePath);
                var settings = JsonSerializer.Deserialize<List<UserSettings>>(userSettings);
                return settings[0].LocalPath ?? string.Empty;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilesAndDirectoriesService GetUserSettings error: {ex}");
                return string.Empty;
            }
        }
    }
}
