using Avalonia.Media;
using FTPClient.Models;
using FTPClient.Service.Interfaces;
using System.Diagnostics;
using System.Text.Json;

namespace FTPClient.Service.Services
{
    public class FilesAndDirectoriesService : IFilesAndDirectoriesService
    {
        private static string settingsFilePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/userSettings.json";
        private static string profileFilePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/profile.json";
        public void SaveUserConfigFile(Profile profile)
        {
            try
            {
                var allProfiles = GetUserSettings();
                var indexOfMyProfile = -1;
                foreach (var p in allProfiles)
                {
                    if(p.Name == profile.Name)
                    {
                        indexOfMyProfile  = allProfiles.IndexOf(p);
                    }
                }
                if (indexOfMyProfile > -1)
                {
                    if (!string.IsNullOrEmpty(profile.ProfileSettings.LocalPath))
                    {
                        allProfiles[indexOfMyProfile].ProfileSettings.LocalPath = profile.ProfileSettings.LocalPath;
                    }
                    var color = profile.ProfileSettings.ProfileColor;
                    if(color != null)
                    {
                        allProfiles[indexOfMyProfile].ProfileSettings.ProfileColor = profile.ProfileSettings.ProfileColor;
                    }
                    string jsonFile = JsonSerializer.Serialize(allProfiles);
                    using (StreamWriter writer = new StreamWriter(settingsFilePath))
                    {
                        writer.Write(jsonFile);
                    }
                }
                else
                {
                    Profile newProfile = new Profile()
                    {
                        Name = profile.Name,
                        ProfileSettings = new ProfileSettings
                        {
                            LocalPath = profile.ProfileSettings.LocalPath,
                            ProfileColor = profile.ProfileSettings.ProfileColor,
                        }
                    };
                    allProfiles.Add(newProfile);

                    string jsonFile = JsonSerializer.Serialize(allProfiles);
                    using (StreamWriter writer = new StreamWriter(settingsFilePath))
                    {
                        writer.Write(jsonFile);
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"FilesAndDirectoriesService SaveUserConfigFile error: {ex}");
            }
            
        }
        public Profile GetUserSettings(string profileName)
        {
            try
            {
                if (!File.Exists(settingsFilePath))
                {
                    using FileStream fs = File.Create(settingsFilePath);
                }
                string userSettings = string.Empty;
                using (StreamReader reader = new StreamReader(settingsFilePath))
                {
                    userSettings = reader.ReadToEnd();
                }
                if (string.IsNullOrWhiteSpace(userSettings))
                {
                    return new Profile();
                }
                else
                {
                    var settings = JsonSerializer.Deserialize<List<Profile>>(userSettings);
                    foreach (Profile profile in settings)
                    {
                        if (profile.Name == profileName)
                        {
                            return profile ?? new Profile();
                        }
                    }
                    return new Profile();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilesAndDirectoriesService GetUserSettings error: {ex}");
                return new Profile();
            }
        }
        public List<Profile> GetUserSettings()
        {
            try
            {
                if (!File.Exists(settingsFilePath))
                {
                    Profile newProfile = new Profile()
                    {
                        Name = "Default",
                        ProfileSettings = new ProfileSettings
                        {
                            LocalPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                            ProfileColor = new JsonColor
                            {
                                R = 36, G = 39, B = 42
                            },
                        }
                    };
                    SaveUserConfigFile(newProfile);
                }
                var userSettings = File.ReadAllText(settingsFilePath);
                var settings = JsonSerializer.Deserialize<List<Profile>>(userSettings);
                return settings ?? new List<Profile>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilesAndDirectoriesService GetUserSettings error: {ex}");
                return new List<Profile>();
            }
        }
        public void SaveCurrentProfile(string profileName)
        {
            try
            {
                string jsonFile = JsonSerializer.Serialize(profileName);
                File.WriteAllText(profileFilePath, jsonFile);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilesAndDirectoriesService SaveCurrentProfileFile error: {ex}");
            }
        }
        public void SaveCurrentProfile()
        {
            try
            {
                string jsonFile = JsonSerializer.Serialize("Default");
                File.WriteAllText(profileFilePath, jsonFile);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilesAndDirectoriesService SaveCurrentProfileFile error: {ex}");
            }
        }
        public string GetCurrentProfile()
        {
            try
            {
                if (!File.Exists(profileFilePath))
                {
                    SaveCurrentProfile("Default");
                }
                var currentProfileJson = File.ReadAllText(profileFilePath);
                var currentProfile = JsonSerializer.Deserialize<string>(currentProfileJson);
                return currentProfile ?? string.Empty;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilesAndDirectoriesService GetCurrentProfile error: {ex}");
                return string.Empty;
            }
        }
        public void AddNewProfile(string newProfileName)
        {
            try
            {
                var profiles = GetUserSettings();
                bool isAlreadyExists = false;
                foreach(var profile in profiles)
                {
                    if(profile.Name == newProfileName)
                    {
                        isAlreadyExists = true;
                    }
                }
                if (!isAlreadyExists)
                {
                    profiles.Add(new Profile()
                    {
                        Name = newProfileName,
                        ProfileSettings = new ProfileSettings()
                        {
                            LocalPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                        }
                    });
                    string jsonFile = JsonSerializer.Serialize(profiles);
                    File.WriteAllText(settingsFilePath, jsonFile);
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"FilesAndDirectoriesService AddNewProfile error: {ex}");
            }
        } 
        public void DeleteProfile(string profileName)
        {
            try
            {
                var profiles = GetUserSettings();
                var profile = profiles.Where(p => p.Name == profileName).FirstOrDefault();
                int indexOfProfile = profiles.IndexOf(profile);
                profiles.RemoveAt(indexOfProfile);

                string jsonFile = JsonSerializer.Serialize(profiles);
                File.WriteAllText(settingsFilePath, jsonFile);
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"FilesAndDirectoriesService AddNewProfile error: {ex}");
            }
        }
    }
}
